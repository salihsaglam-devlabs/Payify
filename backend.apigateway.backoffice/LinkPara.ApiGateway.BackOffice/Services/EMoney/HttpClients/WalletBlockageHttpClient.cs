using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class WalletBlockageHttpClient : HttpClientBase, IWalletBlockageHttpClient
{

    public WalletBlockageHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync(GetWalletBlockageRequest request)
    {
        var url = CreateUrlWithProperties($"v1/WalletBlockage/get-blockages", request);
        var response = await GetAsync(url);

        var walletSummary = await response.Content.ReadFromJsonAsync<PaginatedList<WalletBlockageDto>>();

        return walletSummary ?? throw new InvalidOperationException();
    }

    public async Task AddWalletBlockageAsync(AddWalletBlockageRequest request)
    {
        var response = await PostAsJsonAsync($"v1/WalletBlockage/wallet-blockage", request);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(); 
    }

    public async Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync(AddWalletBlockageDocumentRequest request)
    {
        var response = await PostAsJsonAsync($"v1/WalletBlockage/add-document", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<WalletBlockageDocumentDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return document;
    }

    public async Task RemoveWalletBlockageDocumentAsync(RemoveWalletBlockageDocumentRequest request)
    {
        var response = await PutAsJsonAsync("v1/WalletBlockage/remove-document", request);        
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();

    }

    public async Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync(GetWalletBlockageDocumentRequest request)
    {
        var url = CreateUrlWithProperties($"v1/WalletBlockage/get-documents", request);
        var response = await GetAsync(url);

        var documents = await response.Content.ReadFromJsonAsync<List<WalletBlockageDocumentDto>>();

        return documents ?? throw new InvalidOperationException();
    }
    
}