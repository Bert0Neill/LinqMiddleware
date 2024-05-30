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
        const int MINIMUM_ITEM_SIZE = 2; // as queries can be complex and have multiple IN\NOT IN clause, EF will cascade the IN clauses to other tables, thus it's best to implement the OpenJson at around 1K of values to cover this scenario
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

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, 
            InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
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
            // Define a single regular expression to match both IN and NOT IN clauses
            Regex inClauseRegex = new Regex(@"\b(NOT\s+)?IN\s*\((.*?)\)", RegexOptions.IgnoreCase);

            // Replace IN and NOT IN clauses with the OPENJSON format
            string result = inClauseRegex.Replace(sqlStatement, match =>
            {
                string prefix = match.Groups[1].Value; // Capture 'NOT ' if it exists
                string values = match.Groups[2].Value.Trim(); // Capture the values inside the parentheses
                return $"{prefix}IN ({ConvertToOpenJson(values)})";
            });

            return result;
        }

        private string ConvertToOpenJson(string values)
        {
            const string OPENJSON_SYNTAX = "SELECT value FROM OPENJSON('[ {0} ]') WITH ( {1} )";
            const string OPEN_JSON_NVARCHAR_MAX = "Value NVARCHAR(MAX) '$'";
            const string OPEN_JSON_INT_MAX = "Value INT '$'";
            const string CHAR_PREFIX = "N'";

            List<string> valueList = new();
            string typeList = string.Empty;

            // split the values string by commas and trim each value            
            string[] valueArray = values.Split(','); //  A sun-query inside an IN clause will return a count of 1, like this query: SELECT * FROM[LinqPerformanceDemo].[dbo].[LargeTable] WHERE ID IN (SELECT TOP 10 ID FROM[LinqPerformanceDemo].[dbo].[LargeTable])
            valueArray = valueArray.Select(itm => itm.Trim()).ToArray(); // remove any leading\trailing spaces
            valueArray = valueArray.Select(s => s.StartsWith(CHAR_PREFIX) ? s.Substring(1) : s).ToArray(); // if value starts with N' remove the N. To cater for Char converions of string

            if (valueArray.Length < MINIMUM_ITEM_SIZE) return values;

            // Check if the value is integer, double or default to NVarchar
            string trimmedValue = valueArray[0];
            if (int.TryParse(trimmedValue, out int intValue))
            {
                // For integer values, add them as is
                valueList = valueArray.ToList();
                typeList = OPEN_JSON_INT_MAX;
            }
            else if (double.TryParse(trimmedValue, out double doubleValue))
            {
                // For non-integer numeric values, add them as is (will be cast to NVARCHAR(MAX))
                valueList = valueArray.ToList();
                typeList = OPEN_JSON_NVARCHAR_MAX;
            }
            else // string based
            {
                // remove single quotes from start\end of string and wrap in double quotes and escape any existing quotes
                valueList = valueArray.Select(s => "\"" + s.Trim('\'').Replace("\"", "\\\"") + "\"").ToList();
                typeList = OPEN_JSON_NVARCHAR_MAX;
            }

            return String.Format(OPENJSON_SYNTAX, string.Join(",", valueList), typeList);
        }
        #endregion
    }
}
