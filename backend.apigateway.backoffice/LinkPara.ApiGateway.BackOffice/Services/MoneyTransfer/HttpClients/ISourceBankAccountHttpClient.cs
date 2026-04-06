using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface ISourceBankAccountHttpClient
{
    Task<SourceBankAccountDto> GetByIdAsync(Guid id);
    Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request);
    Task<List<BankModel>> GetAccountBanksAsync(GetAccountBanksRequest request);
    Task SaveAsync(SaveSourceBankAccountRequest request);
    Task UpdateAsync(UpdateSourceBankAccountRequest request);
    Task DeleteAsync(DeleteSourceBankAccountRequest request);
}
