using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace LinqOpenJsonInClause
{
    public interface ILinqContainsOpenJsonMiddleware
    {
        InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result);
        ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default);
    }
}