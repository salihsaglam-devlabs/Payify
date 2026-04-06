using LinkPara.LogConsumers.Commons.Interfaces;
using MassTransit;
using sharedModel = LinkPara.Audit.Models;


namespace LinkPara.LogConsumers.Consumers
{
    public class UserActivityLogConsumer : IConsumer<sharedModel.UserActivityLog>
    {
        private readonly IElasticSearchService _elasticSearchService;
        public UserActivityLogConsumer(
            IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }
        public async Task Consume(ConsumeContext<sharedModel.UserActivityLog> context)
        {
            var indexName = $"user-activity-log-{DateTime.Now.ToString("yyyy.MM.dd")}";
            await _elasticSearchService.
               InsertDocumentAsync(indexName, context.Message);
        }
    }
}