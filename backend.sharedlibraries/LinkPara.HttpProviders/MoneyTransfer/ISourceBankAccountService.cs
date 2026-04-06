using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.MoneyTransfer;

public interface ISourceBankAccountService
{
    Task<PaginatedList<SourceBankAccountDto>> GetAllSourceBankAccountsAsync(GetSourceBankAccountsRequest request);
}