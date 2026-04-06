using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IBankApiHttpClient
{
    Task<BankApiDto> GetByIdAsync(Guid id);
    Task<PaginatedList<BankApiDto>> GetListAsync(GetBankApiListRequest request);
    Task SaveAsync(SaveBankApiRequest request);
    Task UpdateAsync(UpdateBankApiRequest request);
    Task DeleteAsync(DeleteBankApiRequest request);
}
