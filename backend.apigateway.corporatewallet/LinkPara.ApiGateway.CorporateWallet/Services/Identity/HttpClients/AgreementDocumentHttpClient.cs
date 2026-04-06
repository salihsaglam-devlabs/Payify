using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public class AgreementDocumentHttpClient : HttpClientBase, IAgreementDocumentHttpClient
{
    public AgreementDocumentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<AgreementDocumentVersionDto>> GetDocumentsAsync()
    {
        var response = await GetAsync($"v1/AgreementDocuments");
        var responseString = await response.Content.ReadAsStringAsync();

        var agreementDocument = JsonSerializer.Deserialize<List<AgreementDocumentVersionDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return agreementDocument ?? throw new InvalidOperationException();
    }

    public async Task CreateUserDocumentAsync(CreateDocumentToUserRequest request)
    {
        await PostAsJsonAsync($"v1/AgreementDocuments/{request.UserId}", request);
    }
}