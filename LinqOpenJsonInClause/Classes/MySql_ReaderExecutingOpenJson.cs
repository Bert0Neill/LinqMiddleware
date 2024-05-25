using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;
using LinqOpenJsonInClauseMiddleware.Interfaces;

namespace LinqOpenJsonInClauseMiddleware.Classes
{
    public class MySqlReaderExecutingOpenJson : DbCommandInterceptor, ILinqOpenJsonMiddleware
    {
        #region Class variables
        const int MINIMUM_ITEM_SIZE = 100; // as queries can be complex and have multiple IN claues, best to implement OpenJson all the time
        #endregion

        #region Override methods
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToJsonTable(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to JSON_TABLE format
            string modifiedSqlStatement = ConvertInClausesToJsonTable(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }
        #endregion

        #region Helper Methods
        private string ConvertInClausesToJsonTable(string sqlStatement)
        {
            // Define regular expressions to match IN or NOT IN clauses
            Regex inRegex = new Regex(@"\bIN\s*\((.*?)\)", RegexOptions.IgnoreCase);
            Regex notInRegex = new Regex(@"\bNOT\s+IN\s*\((.*?)\)", RegexOptions.IgnoreCase);

            // Find all NOT IN clauses and replace them with JSON_TABLE format
            string result = notInRegex.Replace(sqlStatement, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "NOT IN (" + ConvertToJsonTable(values) + ")";
            });

            // Find all IN clauses and replace them with JSON_TABLE format
            result = inRegex.Replace(result, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "IN (" + ConvertToJsonTable(values) + ")";
            });

            return result;
        }

        private string ConvertToJsonTable(string values)
        {
            List<string> valueList = new List<string>();

            // Split the values string by commas and trim each value
            string[] valueArray = values.Split(',');
            foreach (string value in valueArray)
            {
                // Trim whitespace
                string trimmedValue = value.Trim();

                // Check if the value is numeric (integer or double)
                if (double.TryParse(trimmedValue, out double doubleValue))
                {
                    // For numeric values, just add them as is
                    valueList.Add(trimmedValue);
                }
                else
                {
                    trimmedValue = trimmedValue.Trim('\''); // remove single quotes from start\end of string                    
                    valueList.Add($"'\"{trimmedValue.Replace("\"", "\\\"")}\"'"); // for strings, wrap in double quotes and escape any existing quotes
                }
            }

            string jsonArray = "[" + string.Join(",", valueList) + "]";
            return $"SELECT value FROM JSON_TABLE('{jsonArray}', '$[*]' COLUMNS(value VARCHAR(255) PATH '$')) AS jt";
        }
        #endregion

    }
}
