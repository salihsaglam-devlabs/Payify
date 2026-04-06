using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers
{
    public class ExceptionLogConsumer : IConsumer<Batch<ExceptionLog>>
    {
        private readonly IElasticSearchService _elasticSearchService;

        public ExceptionLogConsumer(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public Task Consume(ConsumeContext<Batch<ExceptionLog>> context)
        {

            var messages = context.Message.ToList();
            var logEvents = messages.Select(message => message.Message.LogEvent).ToList();

            var indexName = $"exception-logs-{DateTime.Now.ToString("yyyy.MM")}";
            _elasticSearchService.BulkPayloadAsync(indexName, logEvents);

            return Task.CompletedTask;
        }
    }
}
