using LinkPara.Emoney.ApiGateway.Models.Responses;

namespace LinkPara.Emoney.ApiGateway.Services.HttpClients;

public interface IApiKeyHttpClient
{
    Task<ApiKeyDto> GetApiKeyAsync(string publicKey);
}
