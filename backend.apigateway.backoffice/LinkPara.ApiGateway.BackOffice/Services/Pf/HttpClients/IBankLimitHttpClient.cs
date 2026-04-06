using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IBankLimitHttpClient
{
    Task<PaginatedList<BankLimitDto>> GetAllAsync(GetFilterBankLimitRequest request);
    Task<BankLimitDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveBankLimitRequest request);
    Task UpdateAsync(UpdateBankLimitRequest request);
    Task DeleteBankLimitAsync(Guid id);
}
