using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IBankHttpClient
{
    Task<PaginatedList<BankDto>> GetAllAsync(GetFilterBankRequest request);
    Task<List<BankApiKeyDto>> GetAllBankApiKeyAsync(Guid acquireBankId);
}
