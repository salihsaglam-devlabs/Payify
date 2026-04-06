using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;
using LinkPara.Emoney.Application.Commons.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IMasterpassService
{
    Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync(GenerateAccessTokenRequest request);
    Task ThreedSecureAsync(ThreedSecureRequest request);
    Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId);
    Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAccountAsync(AccountUnlinkRequest request);
    Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request, Wallet wallet, CardTopupRequest cardTopupRequest, ITopupService topupService);
    Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService);
    Task RefundOrVoidProcessAsync(CardTopupRequest cardTopupRequest, CardTopupRequestStatus voidStatus, CardTopupRequestStatus refundStatus, CardTopupRequestStatus failedStatus, string formattedAmount);
    string FormatAmount(decimal amount);
    Task<CardBinDto> GetCardBinByNumberAsync(string cardNumber);
}
