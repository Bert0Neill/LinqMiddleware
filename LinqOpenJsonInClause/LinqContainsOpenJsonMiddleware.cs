using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;

namespace LinqOpenJsonInClause
{
    public class LinqContainsOpenJsonMiddleware : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            if (command.CommandText.Contains(" IN ("))
            {
                // Check for other includes other than IDs // If SQL string is "... IN (1, 2, 3) ..." we want "... IN (SELECT value FROM OPENJSON('[1, 2, 3]')) ..."
                Regex regex = new Regex(@"IN (\[\d]+[, \d]*\))+");

                command.CommandText = regex.Replace(command.CommandText, s =>
                {
                    if (s.Value.Length > 100) // Sanity check to avoid small lists, like status IDs
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("IN (SELECT value FROM OPENJSON('[");
                        sb.Append(s.Value.Substring(4, s.Value.Length - 5));
                        sb.Append("]') WITH (Value INT '$'))");
                        return sb.ToString(); // return our injected OpenJson style SQL syntax
                    }
                    else
                    {
                        return s.Value; // return the original (LINQ) SQL statement with the small "IN" clause (no need to change syntax)
                    }
                });             
            }
            return result; // return the original (LINQ) SQL statement - no "IN" clause in SQL statement
        }
    }
}
