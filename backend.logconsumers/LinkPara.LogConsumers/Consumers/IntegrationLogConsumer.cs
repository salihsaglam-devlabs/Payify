using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers
{
    public class IntegrationLogConsumer : IConsumer<IntegrationLog>
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IConfidentialService _confidentialHelper;
        public IntegrationLogConsumer(IElasticSearchService elasticSearchService, 
            IConfidentialService confidentialHelper)
        {
            _elasticSearchService = elasticSearchService;
            _confidentialHelper = confidentialHelper;
        }

        public async Task Consume(ConsumeContext<IntegrationLog> context)
        {
            var indexName = $"integration-log-{DateTime.Now.ToString("yyyy.MM.dd")}";

            await _elasticSearchService.
                CreateIndexAsync<IntegrationLog>
                (indexName);

            context.Message.Request = _confidentialHelper.MaskData(context.Message.Request, context.Message.DataType, context.Message.Name);
            context.Message.Response = _confidentialHelper.MaskData(context.Message.Response, context.Message.DataType, context.Message.Name);

            await _elasticSearchService.
                InsertDocumentAsync(indexName, context.Message);
        }
    }
}
