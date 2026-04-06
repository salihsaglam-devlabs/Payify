using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IPfPaymentHttpClient
{
    Task<PfProvisionResponse> SavePaymentAsync(ProvisionRequest provision); 
    Task<PfReturnResponse> ReturnPaymentAsync(ReturnRequest returnRequest); 
    Task<PfReverseResponse> ReversePaymentAsync(ReverseRequest reverse); 
    Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request);
    Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request);
    Task<InquireResponse> InquirePaymentAsync(InquireRequest request);
}
