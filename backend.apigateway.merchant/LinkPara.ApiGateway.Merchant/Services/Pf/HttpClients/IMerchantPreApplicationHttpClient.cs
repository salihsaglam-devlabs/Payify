using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantPreApplicationHttpClient
{
    Task<MerchantPreApplicationResponse> CreatePosApplicationAsync(CreateMerchantPreApplicationRequest request);
}