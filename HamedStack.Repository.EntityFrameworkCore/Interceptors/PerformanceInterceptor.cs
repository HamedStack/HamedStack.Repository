using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HamedStack.TheRepository.EntityFrameworkCore.Interceptors;

public class PerformanceInterceptor : DbCommandInterceptor
{
    private const long SlowQueryThreshold = 100; // milliseconds

    private readonly ILogger _logger;

    public PerformanceInterceptor(ILogger logger)
    {
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThreshold)
        {
            LogQuery(command, eventData);
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThreshold)
        {
            LogQuery(command, eventData);
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogQuery(DbCommand command, CommandExecutedEventData eventData)
    {
        _logger.LogWarning($"SlowQuery:{command.CommandText}.\nTotalMilliseconds:{eventData.Duration.TotalMilliseconds}");
    }
}