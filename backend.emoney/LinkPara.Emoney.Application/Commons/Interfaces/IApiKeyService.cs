using LinkPara.Emoney.Application.Features.Partners;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyDto> GetApiKey(string publicKey);
}
