using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public interface ISavedBillHttpClient
{
    Task SaveAsync(CreateSavedBillRequest request);
    Task UpdateAsync(UpdateSavedBillRequest request);
    Task DeleteAsync(Guid id);
    Task<PaginatedList<SavedBillDto>> GetAllAsync(SavedBillFilterRequest request);
}