using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface IWalletHttpClient
{
    Task<WalletDto> GetWalletDetailsAsync(GetWalletDetailsRequest request);
    Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsFilterRequest request);
    Task SaveWalletAsync(SaveWalletRequest request, string userId = null);
    Task UpdateWalletAsync(UpdateWalletRequest request);
    Task<MoneyTransferResponse> TransferAsync(TransferRequest request);
    Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequest request);
    Task<WalletSummaryDto> GetWalletSummaryAsync(GetWalletSummaryDetailsRequest request);
    Task<WithdrawPreviewResponse> WithdrawPreviewAsync(WithdrawPreviewRequest request);
    Task<TransferPreviewResponse> TransferPreviewAsync(TransferPreviewRequest request);
    Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsRequest request);
    Task<PayWithWalletResponse> TransferForLoggedInUserAsync(TransferForLoggedInUserRequest request);
    Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync();

}