using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public interface ISavedBillHttpClient
{
    Task SaveAsync(CreateSavedBillRequest request);
    Task UpdateAsync(UpdateSavedBillRequest request);
    Task DeleteAsync(Guid id);
    Task<PaginatedList<SavedBillDto>> GetAllAsync(SavedBillFilterRequest request);
}