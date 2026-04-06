using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateBalance;

namespace LinkPara.Emoney.Application.Commons.Interfaces;
public interface IUpdateBalanceService
{
    Task<UpdateBalanceResponse> MoneyInAsync(UpdateBalanceCommand command);
    Task<UpdateBalanceResponse> MoneyOutAsync(UpdateBalanceCommand command);
    Task<UpdateBalanceResponse> ReturnAsync(UpdateBalanceCommand command);
    Task<UpdateBalanceResponse> MaintenanceAsync(UpdateBalanceCommand command);
}
