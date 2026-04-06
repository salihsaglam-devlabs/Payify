using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers;

public class BankAccountActivityConsumer : IConsumer<BankAccountActivity>
{
    private readonly IElasticSearchService _elasticSearchService;

    public BankAccountActivityConsumer(IElasticSearchService elasticSearchService)
    {
        _elasticSearchService = elasticSearchService;
    }

    public async Task Consume(ConsumeContext<BankAccountActivity> context)
    {
        var indexName = $"bank-account-activity-{DateTime.Now.ToString("yyyy.MM.dd")}";

        await _elasticSearchService.
            CreateIndexAsync<BankAccountActivity>
            (indexName);

        await _elasticSearchService.
            InsertDocumentAsync(indexName, context.Message);
    }
}
