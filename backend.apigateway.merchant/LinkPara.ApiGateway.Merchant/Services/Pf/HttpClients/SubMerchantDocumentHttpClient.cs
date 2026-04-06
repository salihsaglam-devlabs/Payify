using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class SubMerchantDocumentHttpClient : HttpClientBase, ISubMerchantDocumentsHttpClient
{
    public SubMerchantDocumentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<SubMerchantDocumentDto>> GetAllAsync(GetAllSubMerchantDocumentRequest request)
    {
        var url = CreateUrlWithParams($"v1/SubMerchantDocuments", request, true);
        var response = await GetAsync(url);
        var subMerchantDocuments = await response.Content.ReadFromJsonAsync<PaginatedList<SubMerchantDocumentDto>>();
        return subMerchantDocuments ?? throw new InvalidOperationException();
    }

    public async Task<SubMerchantDocumentDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SubMerchantDocuments/{id}");
        var subMerchantDocument = await response.Content.ReadFromJsonAsync<SubMerchantDocumentDto>();
        return subMerchantDocument ?? throw new InvalidOperationException();
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/SubMerchantDocuments/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task SaveSubMerchantDocument(SaveSubMerchantDocumentRequest request)
    {
        var response = await PostAsJsonAsync($"v1/SubMerchantDocuments", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateSubMerchantDocument(SubMerchantDocumentDto request)
    {
        var response = await PutAsJsonAsync($"v1/SubMerchantDocuments", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}