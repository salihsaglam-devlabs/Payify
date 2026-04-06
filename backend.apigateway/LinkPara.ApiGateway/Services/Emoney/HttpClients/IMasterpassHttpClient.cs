using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface IMasterpassHttpClient
{
    Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync(GenerateAccessTokenRequest request);
    Task ThreedSecureAsync(ThreedSecureRequest request);
    Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId);
    Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAccountAsync(AccountUnlinkRequest request);
    Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request);
}