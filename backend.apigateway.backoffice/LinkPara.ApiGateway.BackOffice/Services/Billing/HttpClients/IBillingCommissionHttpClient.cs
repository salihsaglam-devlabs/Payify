using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface IBillingCommissionHttpClient
{
    Task<PaginatedList<BillingCommissionDto>> GetCommissionsAsync(GetAllBillingCommissionRequest request);
    Task<BillingCommissionDto> GetCommissionAsync(Guid commissionId);
    Task DeleteCommissionAsync(Guid id);
    Task CreateCommissionAsync(CreateBillingCommissionRequest request);
}