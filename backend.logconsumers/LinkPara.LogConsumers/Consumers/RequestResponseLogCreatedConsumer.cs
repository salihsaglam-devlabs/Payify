using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers;

public class RequestResponseLogCreatedConsumer : IConsumer<RequestResponseLogCreated>
{
    private const string emptyJson = "{}";
    private const char invalidChar = '�';

    private readonly IElasticSearchService _elasticSearchService;
    private readonly IConfidentialService _confidentialHelper;

    public RequestResponseLogCreatedConsumer(IElasticSearchService elasticSearchService
                                , IConfidentialService confidentialHelper)
    {
        _elasticSearchService = elasticSearchService;
        _confidentialHelper = confidentialHelper;
    }

    public async Task Consume(ConsumeContext<RequestResponseLogCreated> context)
    {
        var indexName = $"request-response-log-{DateTime.Now.ToString("yyyy.MM.dd")}";
        await _elasticSearchService.
        CreateIndexAsync<RequestResponseLogCreated>
        (indexName);

        if (context.Message.Response.Response != "Healthy")
        {
            if(context.Message.Response.Response != null && context.Message.Response.Response.Contains("Healthy"))
            {
                return;
            }

            if (context.Message.Response.Response.Contains(invalidChar))
            {
                context.Message.Response.Response = emptyJson;
            }

            context.Message.Request.RequestBody = _confidentialHelper.MaskData(context.Message.Request.RequestBody, IntegrationLogDataType.Json);
            context.Message.Response.Response = _confidentialHelper.MaskData(context.Message.Response.Response, IntegrationLogDataType.Json);

            await _elasticSearchService.
                InsertDocumentAsync(indexName, context.Message);
        }
    }
}

