using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;

namespace LinqOpenJsonInClause
{
    public class LinqContainsOpenJsonMiddleware : DbCommandInterceptor, ILinqContainsOpenJsonMiddleware
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            //return new ValueTask<InterceptionResult<DbDataReader>>(CheckForInClause(command, result));

            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToOpenJson(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            //return CheckForInClause(command, result);

            // Extract the SQL statement from the command
            string sqlStatement = command.CommandText;

            // Convert IN and NOT IN clauses to OPENJSON format
            string modifiedSqlStatement = ConvertInClausesToOpenJson(sqlStatement);

            // Update the command's SQL statement
            command.CommandText = modifiedSqlStatement;

            // Return the interception result
            return result;
        }

       

        //// original code
        //private static string ConvertInClausesToOpenJson(string sqlStatement)
        //{
        //    //// Define regular expressions to match IN or NOT IN clauses
        //    //Regex inRegex = new Regex(@"\bIN\s*\((.*?)\)", RegexOptions.IgnoreCase);
        //    //Regex notInRegex = new Regex(@"\bNOT\s+IN\s*\((.*?)\)", RegexOptions.IgnoreCase);

        //    //// Find all IN clauses and replace them with OPENJSON format
        //    //string result = inRegex.Replace(sqlStatement, match =>
        //    //{
        //    //    string values = match.Groups[1].Value.Trim();
        //    //    return "IN (" + ConvertToOpenJson(values) + ")";
        //    //});

        //    //// Find all NOT IN clauses and replace them with OPENJSON format
        //    //result = notInRegex.Replace(result, match =>
        //    //{
        //    //    string values = match.Groups[1].Value.Trim();
        //    //    return "NOT IN (" + ConvertToOpenJson(values) + ")";
        //    //});

        //    //return result;

        //    // Define regular expressions to match IN or NOT IN clauses
        //    Regex inRegex = new Regex(@"\bIN\s*\(([^)]+)\)", RegexOptions.IgnoreCase);
        //    Regex notInRegex = new Regex(@"\bNOT\s+IN\s*\(([^)]+)\)", RegexOptions.IgnoreCase);

        //    // Find all IN clauses and replace them with OPENJSON format
        //    string result = inRegex.Replace(sqlStatement, match =>
        //    {
        //        string values = match.Groups[1].Value.Trim();
        //        return "IN (" + ConvertToOpenJson(values) + ")";
        //    });

        //    // Find all NOT IN clauses and replace them with OPENJSON format
        //    result = notInRegex.Replace(result, match =>
        //    {
        //        string values = match.Groups[1].Value.Trim();
        //        return "NOT IN (" + ConvertToOpenJson(values) + ")";
        //    });

        //    return result;
        //}

        //private static string ConvertToOpenJson(string values)
        //{
        //    List<string> valueList = new List<string>();

        //    // Split the values string by commas and trim each value
        //    string[] valueArray = values.Split(',');
        //    foreach (string value in valueArray)
        //    {
        //        // Trim whitespace
        //        string trimmedValue = value.Trim();

        //        // Check if the value is numeric (integer or double)
        //        double doubleValue;
        //        bool isNumeric = double.TryParse(trimmedValue, out doubleValue);

        //        if (isNumeric)
        //        {
        //            // For numeric values, just add them as is
        //            valueList.Add(trimmedValue);
        //        }
        //        else if (trimmedValue.Length == 1)
        //        {
        //            // For single characters, convert to ASCII value
        //            valueList.Add(((int)trimmedValue[0]).ToString());
        //        }
        //        else
        //        {
        //            // For strings, wrap in single quotes
        //            valueList.Add("'" + trimmedValue.Replace("'", "''") + "'");
        //        }
        //    }

        //    // Decide whether to use OPENJSON or not based on the number of values
        //    if (valueList.Count > 100)
        //    {
        //        // Construct the OPENJSON format
        //        return "SELECT value FROM OPENJSON('[" + string.Join(",", valueList) + "]') WITH (Value NVARCHAR(MAX) '$')";
        //    }
        //    else
        //    {
        //        // Just return the values
        //        return string.Join(",", valueList);
        //    }
        //}

        // new
        static string ConvertInClausesToOpenJson(string sqlStatement)
        {
            // Define regular expressions to match IN or NOT IN clauses
            Regex inRegex = new Regex(@"\bIN\s*\((.*?)\)", RegexOptions.IgnoreCase);
            Regex notInRegex = new Regex(@"\bNOT\s+IN\s*\((.*?)\)", RegexOptions.IgnoreCase);

            // Find all IN clauses and replace them with OPENJSON format
            string result = inRegex.Replace(sqlStatement, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "IN (" + ConvertToOpenJson(values) + ")";
            });

            // Find all NOT IN clauses and replace them with OPENJSON format
            result = notInRegex.Replace(result, match =>
            {
                string values = match.Groups[1].Value.Trim();
                return "NOT IN (" + ConvertToOpenJson(values) + ")";
            });

            return result;
        }

        static string ConvertToOpenJson(string values)
        {
            List<string> valueList = new List<string>();

            // Split the values string by commas and trim each value
            string[] valueArray = values.Split(',');
            foreach (string value in valueArray)
            {
                // Trim whitespace
                string trimmedValue = value.Trim();

                // Check if the value is numeric (integer or double)
                double doubleValue;
                bool isNumeric = double.TryParse(trimmedValue, out doubleValue);

                if (isNumeric)
                {
                    // For numeric values, just add them as is
                    valueList.Add(trimmedValue);
                }
                else if (trimmedValue.Length == 1)
                {
                    // For single characters, convert to ASCII value
                    valueList.Add(((int)trimmedValue[0]).ToString());
                }
                else
                {
                    // For strings, wrap in single quotes
                    valueList.Add("'" + trimmedValue.Replace("'", "''") + "'");
                }
            }

            // Decide whether to use OPENJSON or not based on the number of values
            if (valueList.Count > 100)
            {
                // Construct the OPENJSON format
                return "SELECT value FROM OPENJSON('[" + string.Join(",", valueList) + "]') WITH (Value NVARCHAR(MAX) '$')";
            }
            else
            {
                // Just return the values
                return string.Join(",", valueList);
            }
        }

    }
}
