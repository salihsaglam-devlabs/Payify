using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPaymentProviderService
{
    Task<PaymentProviderProvisionResponse> ProvisionAsync(PaymentProviderProvisionRequest request);
    Task<ReverseResponse> ReverseAsync(ReverseRequest request);
    Task<ReturnResponse> ReturnAsync(ReturnRequest request);
    Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request);
    Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request);
    Task<Init3dsResponse> Init3dsAsync(Init3dsRequest request);
    Task<CardTokenDto> GenerateCardTokenAsync(GenerateCardTokenRequest request);
    Task<MerchantApiKeyDto> GetApiKeysAsync(string publicKey, Uri clientBaseAddress);
    Task<InquireResponseModel> InquireAsync(InquireRequestModel request);
    Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request, Wallet wallet, CardTopupRequest cardTopupRequest, ITopupService topupService);
    Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService);
}
