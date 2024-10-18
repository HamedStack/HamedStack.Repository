using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

/// <summary>
/// A database command interceptor that logs slow queries based on a defined threshold (100 milliseconds).
/// </summary>
public class PerformanceInterceptor : DbCommandInterceptor
{
    /// <summary>
    /// The threshold for query execution time in milliseconds. Queries exceeding this threshold are considered slow.
    /// </summary>
    private const long SlowQueryThreshold = 100; // milliseconds

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceInterceptor"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used to log slow queries.</param>
    public PerformanceInterceptor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Intercepts the execution of a database command and logs it if the query duration exceeds the slow query threshold.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> that was executed.</param>
    /// <param name="eventData">The <see cref="CommandExecutedEventData"/> containing information about the command execution.</param>
    /// <param name="result">The result of the executed command.</param>
    /// <returns>The <see cref="DbDataReader"/> representing the result of the command.</returns>
    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThreshold)
        {
            LogQuery(command, eventData);
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    /// <summary>
    /// Asynchronously intercepts the execution of a database command and logs it if the query duration exceeds the slow query threshold.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> that was executed.</param>
    /// <param name="eventData">The <see cref="CommandExecutedEventData"/> containing information about the command execution.</param>
    /// <param name="result">The result of the executed command.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the executed command.</returns>
    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThreshold)
        {
            LogQuery(command, eventData);
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Logs a query that exceeded the slow query threshold.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> that was executed.</param>
    /// <param name="eventData">The <see cref="CommandExecutedEventData"/> containing information about the command execution.</param>
    private void LogQuery(DbCommand command, CommandExecutedEventData eventData)
    {
        _logger.LogWarning($"SlowQuery: {command.CommandText}.\nTotalMilliseconds: {eventData.Duration.TotalMilliseconds}");
    }
}
