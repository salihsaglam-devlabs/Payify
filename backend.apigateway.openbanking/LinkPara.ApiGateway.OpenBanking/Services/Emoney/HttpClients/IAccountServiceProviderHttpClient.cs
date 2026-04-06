using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using static LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses.UserAccountResultDto;

namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.HttpClients;

public interface IAccountServiceProviderHttpClient
{
    Task<BaseServiceResponse<CustomerConfirmationResponse>> CustomerConfirmationAsync(CustomerConfirmationRequest request);
    Task<AccountTransactionsDto> GetAccountTransactionsAsync(GetAccountTransactionsRequest request);
    Task<List<ChangedBalanceDto>> GetChangedBalanceAsync();
    Task<SendNotificationResultDto> SendGkdNotificationAsync(SendGkdNotificationRequest request);
    Task<SendOtpMessageResultDto> SendOtpMessageAsync(SendOtpMessageRequest request);
    Task<BaseServiceResponse<PaymentContractDto>> CreatePaymentOrderAsync(CreatePaymentOrderRequest request);
    Task<BaseServiceResponse<PaymentContractDto>> PaymentOrderInquiryAsync(PaymentOrderInquiryRequest request);
    Task<GetApprovalScreenWalletListResponse> GetApprovalScreenWalletsAsync(GetApprovalScreenWalletListRequest request);
    Task<GetWalletListResponse> GetWalletsAsync(GetWalletInfoRequest request);
    Task<GetWalletBalanceListResponse> GetWalletBalancesAsync(GetWalletInfoRequest request);
    Task<IdentityInfoDto> GetUserIdentityInfoAsync(GetUserIdentityInfoRequest request);
    Task<List<AccountDetail>> GetUserAccountListAsync(GetUserAccountListRequest request);

}
