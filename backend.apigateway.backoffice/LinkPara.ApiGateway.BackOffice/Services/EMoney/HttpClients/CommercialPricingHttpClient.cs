using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class CommercialPricingHttpClient : HttpClientBase, ICommercialPricingHttpClient
{
    public CommercialPricingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<PricingCommercialDto>> GetAll(CommercialPricingFilterRequest request)
    {
        var url = CreateUrlWithParams("v1/CommercialPricing/all", request, true);
        var response = await GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PaginatedList<PricingCommercialDto>>();
    }

    public async Task CreatePricingCommercial(SaveCommercialPricingRequest request)
    {
        var response = await PostAsJsonAsync($"v1/CommercialPricing/create", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdatePricingCommercial(UpdateCommercialPricingRequest request)
    {
        var response = await PutAsJsonAsync($"v1/CommercialPricing/update", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeletePricingCommercial(Guid id)
    {
        var response = await DeleteAsync($"v1/CommercialPricing/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}