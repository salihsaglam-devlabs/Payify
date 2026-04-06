using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IBankAccountBalancesHttpClient
{
    Task<PaginatedList<BankAccountBalanceDto>> GetBankAccountBalancesAsync(BankAccountBalanceRequest request);
}
