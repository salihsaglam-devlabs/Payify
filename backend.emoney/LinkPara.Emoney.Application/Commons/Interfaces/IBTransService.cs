using LinkPara.Emoney.Application.Commons.Models.BTransModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.BTrans;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IBTransService
{
    public Task SaveMoneyTransferAsync(MoneyTransferReport moneyTransfer);
    public Task SaveBalanceInformationAsync(BalanceInformationReport balanceInformation);
    public Task<BTransIdentity> GetCustomerInformationAsync(Guid customerId);
    public BTransIdentity GetAccountInformation(Account account);
}