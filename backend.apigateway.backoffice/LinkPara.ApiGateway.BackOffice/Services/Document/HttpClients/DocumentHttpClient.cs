using LinkPara.ApiGateway.BackOffice.Services.Document.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Document.HttpClients
{
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

        public async Task<PaginatedList<DocumentResponse>> GetDocumentsAsync(GetDocumentsRequest request)
        {
            var url = CreateUrlWithParams($"v1/Documents", request, true);
            var response = await GetAsync(url);
            var documents = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentResponse>>();
            return documents ?? throw new InvalidOperationException();
        }

        public async Task<DocumentResponse> SaveDocumentAsync(DocumentDto request)
        {
            var result = await PostAsJsonAsync("v1/Documents", request);
            var document = await result.Content.ReadFromJsonAsync<DocumentResponse>();
            return document ?? throw new InvalidOperationException();
        }

        public async Task DeleteDocumentAsync(Guid id)
        {
            await DeleteAsync("v1/Documents/" + id);
        }

        public async Task<DocumentResponse> UpdateDocumentAsync(UpdateDocumentDto request)
        {
            var result = await PutAsJsonAsync("v1/Documents", request);
            var document = await result.Content.ReadFromJsonAsync<DocumentResponse>();
            return document ?? throw new InvalidOperationException();
        }
    }
}
