using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IWalletHttpClient
{
    Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsRequest request);
    Task<WalletBalanceResponse> GetWalletBalancesAsync(GetWalletBalanceRequest request);
    Task ConvertUserWalletsToIndividualAsync(ConvertUserWalletsToIndividualRequest request);
    Task UpdateWalletAsync(UpdateWalletRequest request);
    Task UpdateUserWalletsAsync(Guid userId, UpdateUserWalletsRequest request);
    Task<WalletSummeryDto> GetWalletSummaryAsync(GetWalletSummaryRequest request);
    Task<WalletBalanceDailyResponse> GetWalletBalanceDailyAsync(GetWalletBalancesDailyRequest request);
}