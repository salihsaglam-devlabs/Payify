using LinkPara.ApiGateway.Merchant.Services.Document.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Document.HttpClients
{
    public class DocumentTypeHttpClient : HttpClientBase, IDocumentTypeHttpClient
    {
        public DocumentTypeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {
        }

        public async Task<DocumentTypeDto> GetDocumentTypeAsync(Guid id)
        {
            var response = await GetAsync($"v1/DocumentTypes/" + id);
            var type = await response.Content.ReadFromJsonAsync<DocumentTypeDto>();
            return type ?? throw new InvalidOperationException();
        }

        public async Task<PaginatedList<DocumentTypeDto>> GetDocumentTypesAsync(GetDocumentTypesRequest request)
        {
            var url = CreateUrlWithParams($"v1/DocumentTypes", request, true);
            var response = await GetAsync(url);
            var documents = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentTypeDto>>();
            return documents ?? throw new InvalidOperationException();
        }

        public async Task CreateDocumentTypeAsync(DocumentTypeDto request)
        {
            await PostAsJsonAsync("v1/DocumentTypes", request);
        }

        public async Task DeleteDocumentTypeAsync(Guid id)
        {
            await DeleteAsync($"v1/DocumentTypes/" + id);
        }
    }
}
