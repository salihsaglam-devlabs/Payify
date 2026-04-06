using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public class SavedBillHttpClient : HttpClientBase, ISavedBillHttpClient
{
    public SavedBillHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteAsync(Guid id)
    {
        await DeleteAsync($"v1/SavedBills/{id}");
    }

    public async Task<PaginatedList<SavedBillDto>> GetAllAsync(SavedBillFilterRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/SavedBills" + queryString);

        return await response.Content.ReadFromJsonAsync<PaginatedList<SavedBillDto>>();
    }

    public async Task SaveAsync(CreateSavedBillRequest request)
    {
        await PostAsJsonAsync($"v1/SavedBills", request);
    }

    public async Task UpdateAsync(UpdateSavedBillRequest request)
    {
        await PutAsJsonAsync($"v1/SavedBills", request);
    }
}