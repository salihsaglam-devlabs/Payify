using LinkPara.ApiGateway.CorporateWallet.Services.Document.Models;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Document.HttpClients;

public class DocumentHttpClient : HttpClientBase, IDocumentHttpClient
{
    public DocumentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<DocumentDto> GetDocumentAsync(Guid Id)
    {
        var response = await GetAsync($"v1/Documents/{Id}");
        return await response.Content.ReadFromJsonAsync<DocumentDto>() ?? throw new InvalidOperationException();
    }
}
