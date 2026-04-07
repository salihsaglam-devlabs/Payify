using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.ConvertUserWalletsToIndividual;
using LinkPara.Emoney.Application.Features.Wallets.Commands.SaveWallet;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetAccountWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalanceDaily;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalances;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletSummaries;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetWalletDetailsAsync(GetWalletDetailsQuery query);
    Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsFilterQuery query);
    Task<WalletSummaryDto> GetWalletSummaryAsync(GetWalletSummaryQuery query);
    Task<WalletBalanceResponse> GetWalletBalancesAsync(GetWalletBalancesQuery query);
    Task SaveWalletAsync(SaveWalletCommand command);
    Task UpdateWalletAsync(UpdateWalletCommand command, CancellationToken cancellationToken);
    Task ConvertUserWalletsToIndividualAsync(ConvertUserWalletsToIndividualCommand command);
    Task UpdateUserWalletsAsync(Guid userId, UpdateUserWalletsCommand request);
    Task<bool> IsBalanceSufficientAsync(string walletNumber, decimal amount, bool isCredit);
    Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsQuery query);
    Task<WalletPartnerDto> GetWalletDetailsPartnerAsync(GetWalletDetailsPartnerQuery query);
    Task SyncWalletBalanceDailyAsync();
    Task<WalletBalanceDailyResponse> GetWalletBalanceDailyAsync(GetWalletBalancesDailyQuery query);
    Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync();
}
