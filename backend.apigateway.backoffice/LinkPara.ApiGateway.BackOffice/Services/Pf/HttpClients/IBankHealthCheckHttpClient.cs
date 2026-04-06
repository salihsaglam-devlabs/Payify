using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IBankHealthCheckHttpClient
{
    Task<PaginatedList<BankHealthCheckDto>> GetAllAsync(GetFilterBankHealthCheckRequest request);
    Task UpdateAsync(UpdateBankHealthCheckRequest request);
}
