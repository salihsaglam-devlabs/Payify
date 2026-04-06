using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using MassTransit;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class CardBinHttpClient : HttpClientBase, ICardBinHttpClient
{
    public CardBinHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : 
        base(client, httpContextAccessor)
    {
    }

    public async Task DeleteCardBinAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/CardBins/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CardBinDto>> GetAllAsync(GetAllCardBinRequest request)
    {
        var url = CreateUrlWithParams($"v1/CardBins", request, true);
        var response = await GetAsync(url);
        var pricingProfiles = await response.Content.ReadFromJsonAsync<PaginatedList<CardBinDto>>();
        return pricingProfiles ?? throw new InvalidOperationException();
    }

    public async Task<CardBinDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/CardBins/{id}");
        var cardBin = await response.Content.ReadFromJsonAsync<CardBinDto>();
        return cardBin ?? throw new InvalidOperationException();
    }

    public async Task<UpdateCardBinRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateCardBinRequest> cardBinPatch)
    {
        var response = await PatchAsync($"v1/CardBins/{id}", cardBinPatch);
        var cardBin = await response.Content.ReadFromJsonAsync<UpdateCardBinRequest>();
        return cardBin ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveCardBinRequest request)
    {
        var response = await PostAsJsonAsync($"v1/CardBins", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateCardBinRequest request)
    {
        var response = await PutAsJsonAsync($"v1/CardBins", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
