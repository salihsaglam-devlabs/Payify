using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public interface IMerchantHttpClient
{
    Task<CreateBoaMerchantResponse> CreateBoaMerchantAsync(CreateBoaMerchantRequest request);
    Task<BoaMerchantDto> GetBoaMerchantAsync(string merchantNumber);
}