using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class MerchantContentHttpClient : HttpClientBase, IMerchantContentHttpClient
{
    public MerchantContentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<MerchantContentDto>> GetAllMerchantContentAsync(GetFilterMerchantContentRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantContents", request);
        var response = await GetAsync(url);
        var linkContentList = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantContentDto>>();
        return linkContentList ?? throw new InvalidOperationException();
    }
    
    public async Task<MerchantContentDto> GetMerchantContentByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantContents/{id}");
        var linkContent = await response.Content.ReadFromJsonAsync<MerchantContentDto>();
        return linkContent ?? throw new InvalidOperationException();
    }
    
    public async Task CreateMerchantContentAsync(CreateMerchantContentRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantContents", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task PutMerchantContentAsync(MerchantContentDto request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantContents", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task DeleteMerchantContentAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/MerchantContents/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    
    public async Task<MerchantLogoDto> GetMerchantLogoAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/MerchantContents/logo/{merchantId}");
        var linkLogo = await response.Content.ReadFromJsonAsync<MerchantLogoDto>();
        return linkLogo ?? throw new InvalidOperationException();
    }
    
    public async Task UploadMerchantLogoAsync(MerchantLogoDto merchantLogo)
    {
        var response = await PostAsJsonAsync($"v1/MerchantContents/logo", merchantLogo);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}