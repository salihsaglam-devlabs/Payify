using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace LinkPara.SharedModels.Persistence;

public class LongQueryLogger : DbCommandInterceptor
{
    private readonly IBus _bus;
    private readonly IConfiguration _configuration;
    public LongQueryLogger(IBus bus, IConfiguration configuration)
    {
        _bus = bus;
        _configuration = configuration;
    }
    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        double duration = _configuration.GetValue<double>("LongQueryLogger:MaxDurationLimit");

        if (eventData.Duration.TotalMilliseconds > duration)
        {
            _ = LogLongQueryAsync(eventData, result);
        }
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private async Task LogLongQueryAsync(CommandExecutedEventData eventData, DbDataReader result)
    {
        var correlationId = Guid.NewGuid();
        var log = new LongQueryLog()
        {
            CorrelationId = correlationId.ToString(),
            DatabaseName = eventData.Command.Connection.Database,
            Date = DateTime.Now,
            CommandText = eventData.Command.CommandText,
            Parameters = GetDbParameters(eventData),
            DurationInMilliseconds = Convert.ToDecimal(eventData.Duration.TotalMilliseconds),
            HasErrors = result.GetSchemaTableAsync().Result?.HasErrors ?? false,
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty,
            DataType = IntegrationLogDataType.Text
        };
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.LongQueryLog"));
        await endpoint.Send(log, cancellationToken.Token);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        double duration = _configuration.GetValue<double>("LongQueryLogger:MaxDurationLimit");

        if (eventData.Duration.TotalMilliseconds > duration)
        {
            _ = LogLongQueryAsync(eventData, result);
        }
        return base.ReaderExecuted(command, eventData, result);
    }

    private static List<string> GetDbParameters(CommandExecutedEventData eventData)
    {
        var result = new List<string>();
        var list = eventData.Command.Parameters;
        for (int i = 0; i < list.Count; i++)
        {
            result.Add(string.Concat(list[i].ParameterName, " = ", list[i].Value));
        }

        return result;
    }
}