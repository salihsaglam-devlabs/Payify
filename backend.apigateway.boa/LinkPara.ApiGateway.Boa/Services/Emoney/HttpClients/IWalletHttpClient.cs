using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface IWalletHttpClient
{
    Task<WalletDto> GetWalletDetailsAsync(GetWalletDetailsRequest request);
    Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsFilterRequest request);
    Task UpdateWalletAsync(UpdateWalletRequest request);
    Task<WalletSummaryDto> GetWalletSummaryAsync(GetWalletSummaryDetailsRequest request);
    Task<TransferPreviewResponse> TransferPreviewAsync(TransferPreviewRequest request);
    Task<MoneyTransferResponse> TransferAsync(TransferRequest request);
    Task<WithdrawPreviewResponse> WithdrawPreviewAsync(WithdrawPreviewRequest request);
    Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequest request);
    Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsRequest request);
    Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceRequest request);
    Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync();
}
