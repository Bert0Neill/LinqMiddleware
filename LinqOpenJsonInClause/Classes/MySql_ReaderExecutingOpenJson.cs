using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;
using LinqOpenJsonInClauseMiddleware.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LinqOpenJsonInClauseMiddleware.Classes
{
    internal class MySqlReaderExecutingOpenJson : DbCommandInterceptor, ILinqOpenJsonMiddleware
    {
        #region Class variables
        const int MINIMUM_ITEM_SIZE = 1000; // as queries can be complex and have multiple IN claues, best to implement OpenJson all the time
        #endregion

        #region Override methods
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToJsonContains(sqlStatement);

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
            string modifiedSqlStatement = ConvertInClausesToJsonContains(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }
        #endregion

        #region Helper Methods

        public static string ConvertInClausesToJsonContains(string tsqlQuery)
        {
            // Regex to match IN clauses and extract the column name and the values
            string inClausePattern = @"\b(?<column>\w+)\b IN \((?<values>[\d,\s]+)\)";
            string notInClausePattern = @"\b(?<column>\w+)\b NOT IN \((?<values>'?\w+'?(, '?\w+'?)*)\)";

            // Replace IN clauses with JSON_CONTAINS
            tsqlQuery = Regex.Replace(tsqlQuery, inClausePattern, match =>
            {
                string column = match.Groups["column"].Value;
                string values = match.Groups["values"].Value;
                string jsonArray = $"[{string.Join(", ", values.Split(',').Select(v => v.Trim()))}]";
                return $"JSON_CONTAINS(CAST('{jsonArray}' AS JSON), CAST({column} AS JSON))";
            });

            // Replace NOT IN clauses with NOT JSON_CONTAINS
            tsqlQuery = Regex.Replace(tsqlQuery, notInClausePattern, match =>
            {
                string column = match.Groups["column"].Value;
                string values = match.Groups["values"].Value;
                string jsonArray = $"[{string.Join(", ", values.Split(',').Select(v => $"\"{v.Trim().Trim('\'')}\""))}]";
                return $"NOT JSON_CONTAINS(CAST('{jsonArray}' AS JSON), JSON_QUOTE({column}))";
            });

            // Adjust for table aliases and ensure proper syntax
            tsqlQuery = tsqlQuery.Replace("`", ""); // Remove any backticks
            tsqlQuery = Regex.Replace(tsqlQuery, @"(\s|\()l\.", " "); // Remove table alias prefixes in function calls

            return tsqlQuery;
        }

        //private string ConvertInClausesToJsonContains(string sqlQuery)
        //{

        //    // Regex pattern to match IN and NOT IN clauses
        //    string pattern = @"(\w+)\s+(NOT\s+)?IN\s*\(([^)]+)\)";
        //    MatchCollection matches = Regex.Matches(sqlQuery, pattern, RegexOptions.IgnoreCase);

        //    foreach (Match match in matches)
        //    {
        //        string field = match.Groups[1].Value;
        //        string notInClause = match.Groups[2].Value; // "NOT " if it's a NOT IN clause, otherwise empty
        //        string values = match.Groups[3].Value;
        //        List<string> valueList = new List<string>(values.Split(new[] { ", " }, StringSplitOptions.None));

        //        // Construct JSON array string
        //        string jsonArray = string.Join(", ", valueList);

        //        // Construct the JSON_CONTAINS or NOT JSON_CONTAINS clause with casting
        //        string jsonContainsClause;
        //        if (string.IsNullOrEmpty(notInClause))
        //        {
        //            jsonContainsClause = $"JSON_CONTAINS(CAST('[{jsonArray}]' AS JSON), CAST({field} AS JSON))";
        //        }
        //        else
        //        {
        //            // Check for NULL values in addition to JSON_CONTAINS
        //            jsonContainsClause = $"NOT JSON_CONTAINS(CAST('[{jsonArray}]' AS JSON), CAST({field} AS JSON)) OR {field} IS NULL";
        //        }

        //        // Replace the original IN/NOT IN clause with JSON_CONTAINS/NOT JSON_CONTAINS
        //        sqlQuery = sqlQuery.Replace(match.Value, jsonContainsClause);
        //    }

        //    return sqlQuery;
        //}

        //private string ConvertToJSONArray(string values)
        //{
        //    // Split the values string by commas and trim each value
        //    string[] valueArray = values.Split(',');
        //    List<string> jsonValues = new List<string>();

        //    foreach (string value in valueArray)
        //    {
        //        // Trim whitespace
        //        string trimmedValue = value.Trim();

        //        // Check if the value is numeric (integer or double)
        //        if (double.TryParse(trimmedValue, out _))
        //        {
        //            // For numeric values, just add them as is
        //            jsonValues.Add(trimmedValue);
        //        }
        //        else
        //        {
        //            trimmedValue = trimmedValue.Trim('\''); // remove single quotes from start/end of string
        //            jsonValues.Add($"JSON_QUOTE(\"{trimmedValue}\")"); // wrap string values with JSON_QUOTE
        //        }
        //    }

        //    string jsonArray = $"JSON_ARRAY({string.Join(",", jsonValues)})";
        //    return jsonArray;
        //}



        #endregion
    }
}
