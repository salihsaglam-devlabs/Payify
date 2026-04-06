using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ITopupHttpClient
{
    Task<TopupPreviewResponse> GetTopupPreviewAsync(TopupPreviewRequest request);
    Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request);
    Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request);
    Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request);
    Task<Init3dsResponse> Init3dsAsync(Init3dsRequest request);
    Task<CardTokenResponse> GenerateCardTokenAsync(GenerateCardTokenRequest request);
}