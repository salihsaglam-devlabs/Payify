using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public class AgreementDocumentHttpClient : HttpClientBase, IAgreementDocumentHttpClient
{
    public AgreementDocumentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task CreateDocumentAsync(CreateDocumentRequest request)
    {
        await PostAsJsonAsync("v1/AgreementDocuments/", request);
    }
    public async Task<AgreementDocumentResponse> GetAgreementDocumentByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/AgreementDocuments/{id}");
        var document = await response.Content.ReadFromJsonAsync<AgreementDocumentResponse>();
        return document ?? throw new InvalidOperationException();
    }
    public async Task<PaginatedList<AgreementDocumentResponse>> GetAllAgreementDocumentAsync(GetAllAgreementDocumentRequest request)
    {
        var url = CreateUrlWithParams($"v1/AgreementDocuments/getAllDocument", request, true);
        var response = await GetAsync(url);
        var documents = await response.Content.ReadFromJsonAsync<PaginatedList<AgreementDocumentResponse>>();
        return documents ?? throw new InvalidOperationException();
    }
    public async Task UpdateAsync(UpdateAgreementDocumentRequest request)
    {
        var response = await PutAsJsonAsync($"v1/AgreementDocuments", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<PaginatedList<AgreementUserDto>> GetAgreedUsersOfDocument(AgreedUserListRequest request)
    {
        var url = CreateUrlWithParams($"v1/AgreementDocuments/agreed-user-list", request, true);
        var response = await GetAsync(url);
        var userAgreementList = await response.Content.ReadFromJsonAsync<PaginatedList<AgreementUserDto>>();

        if (!CanSeeSensitiveData())
        {
            userAgreementList.Items.ForEach(s =>
            {
                s.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", s.FirstName);
                s.LastName = SensitiveDataHelper.MaskSensitiveData("Name", s.LastName);
            });
        }

        return userAgreementList ?? throw new InvalidOperationException();
    }
    public async Task DeleteAgreementDocumentAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/AgreementDocuments/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}