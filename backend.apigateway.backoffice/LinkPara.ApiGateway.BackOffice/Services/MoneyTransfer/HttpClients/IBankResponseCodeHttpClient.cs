using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IBankResponseCodeHttpClient
{
    Task<BankResponseCodeDto> GetByIdAsync(Guid id);
    Task<PaginatedList<BankResponseCodeDto>> GetListAsync(GetBankResponseCodeListRequest request);
    Task SaveAsync(SaveBankResponseCodeRequest request);
    Task UpdateAsync(UpdateBankResponseCodeRequest request);
    Task DeleteAsync(DeleteBankResponseCodeRequest request);
}
