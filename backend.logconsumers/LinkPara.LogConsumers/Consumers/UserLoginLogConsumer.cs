using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers;

public class UserLoginLogConsumer : IConsumer<UserLoginLog>
{
    private readonly IElasticSearchService _elasticSearchService;

    public UserLoginLogConsumer(IElasticSearchService elasticSearchService)
    {
        _elasticSearchService = elasticSearchService;
    }

    public async Task Consume(ConsumeContext<UserLoginLog> context)
    {
        var indexName = $"user-login-logs-{DateTime.Now.ToString("yyyy.MM.dd")}";

        await _elasticSearchService.
            CreateIndexAsync<UserLoginLog>
            (indexName);

        await _elasticSearchService.
            InsertDocumentAsync(indexName, context.Message);
    }
   
}
