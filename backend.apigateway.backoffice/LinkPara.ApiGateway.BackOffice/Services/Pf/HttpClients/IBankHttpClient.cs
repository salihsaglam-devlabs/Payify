using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IBankHttpClient
{
    Task<PaginatedList<BankDto>> GetAllAsync(GetFilterBankRequest request);
    Task<List<BankApiKeyDto>> GetAllBankApiKeyAsync(Guid acquireBankId);
}
