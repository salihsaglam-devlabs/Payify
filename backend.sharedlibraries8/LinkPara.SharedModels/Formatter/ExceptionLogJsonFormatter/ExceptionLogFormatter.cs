using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Elasticsearch;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

namespace LinkPara.SharedModels.Formatter.ExceptionLogJsonFormatter;

public class ExceptionLogFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent is null || output is null)
        {
            return;
        }
        var elasticSearchFormatter = new ElasticsearchJsonFormatter();
        var elasticOutput = new StringWriter();
        elasticSearchFormatter.Format(logEvent, elasticOutput);
        var message = elasticOutput.ToString();

        ExceptionLog payifyLogEvent = new ExceptionLog
        {
            LogEvent = message
        };

        var serializedLog = System.Text.Json.JsonSerializer.Serialize(payifyLogEvent);

        output.Write(serializedLog);
    }
}
