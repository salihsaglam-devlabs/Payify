using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers
{
    public class LongQueryLogConsumer : IConsumer<LongQueryLog>
    {
        private readonly IElasticSearchService _elasticSearchService;
        public LongQueryLogConsumer(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public async Task Consume(ConsumeContext<LongQueryLog> context)
        {
            var indexName = $"long-query-log-{DateTime.Now.ToString("yyyy.MM.dd")}";

            await _elasticSearchService.
                CreateIndexAsync<LongQueryLog>
                (indexName);
            
            await _elasticSearchService.
                InsertDocumentAsync(indexName, context.Message);
        }
    }
}
