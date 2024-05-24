using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;
using LinqOpenJsonInClauseMiddleware.Interfaces;

namespace LinqOpenJsonInClauseMiddleware.Classes
{
    public class SqlServerReaderExecutingOpenJson : DbCommandInterceptor, ILinqOpenJsonMiddleware
    {
        #region Class variables
        const int MINIMUM_ITEM_SIZE = 100; // don't generate OpenJson clause if there are less than 100 items within the IN\NOT IN clause
        #endregion

        #region Override methods
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToOpenJson(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToOpenJson(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }
        #endregion

        #region Helper Methods
        private string ConvertInClausesToOpenJson(string sqlStatement)
        {
            // Define regular expressions to match IN or NOT IN clauses
            Regex inRegex = new Regex(@"\bIN\s*\((.*?)\)", RegexOptions.IgnoreCase);
            Regex notInRegex = new Regex(@"\bNOT\s+IN\s*\((.*?)\)", RegexOptions.IgnoreCase);

            // Find all NOT IN clauses and replace them with OPENJSON format
            string result = notInRegex.Replace(sqlStatement, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "NOT IN (" + ConvertToOpenJson(values) + ")";
            });

            // Find all IN clauses and replace them with OPENJSON format
            result = inRegex.Replace(result, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "IN (" + ConvertToOpenJson(values) + ")";
            });

            return result;
        }

        private string ConvertToOpenJson(string values)
        {
            List<string> valueList = new List<string>();

            // Split the values string by commas and trim each value
            string[] valueArray = values.Split(',');
            if (valueArray.Length < MINIMUM_ITEM_SIZE) return values;


            foreach (string value in valueArray)
            {
                // Trim whitespace
                string trimmedValue = value.Trim(); // remove any leading\trailing spaces

                // Check if the value is numeric (integer or double)
                if (double.TryParse(trimmedValue, out double doubleValue))
                {
                    // For numeric values, just add them as is
                    valueList.Add(trimmedValue);
                }
                else
                {
                    trimmedValue = trimmedValue.Trim('\''); // remove single quotes from start\end of string                    
                    valueList.Add("\"" + trimmedValue.Replace("\"", "\\\"") + "\""); // for strings, wrap in double quotes and escape any existing quotes
                }
            }

            return "SELECT value FROM OPENJSON('[" + string.Join(",", valueList) + "]') WITH (Value NVARCHAR(MAX) '$')";
        }
        #endregion
    }
}
