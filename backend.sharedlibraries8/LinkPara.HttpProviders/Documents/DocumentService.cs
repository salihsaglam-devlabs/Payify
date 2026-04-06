using LinkPara.HttpProviders.Documents.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using LinkPara.HttpProviders.BTrans.Models;

namespace LinkPara.HttpProviders.Documents;

public class DocumentService : HttpClientBase, IDocumentService
{

    public DocumentService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<CreateDocumentResponse> CreateDocument(CreateDocumentRequest request)
    {
        var response = await PostAsJsonAsync("v1/Documents", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var documentResponse = await response.Content.ReadFromJsonAsync<CreateDocumentResponse>();

        return documentResponse ?? throw new InvalidOperationException();

    }

    public async Task<PaginatedList<GetDocumentResponse>> GetDocuments(GetDocumentListRequest request)
    {
        var url = GetQueryString.CreateUrlWithParams($"v1/Documents", request);

        var response = await GetAsync(url);

        var documentResponse = await response.Content.ReadFromJsonAsync<PaginatedList<GetDocumentResponse>>();

        return documentResponse ?? throw new InvalidOperationException();
    }
    
    public async Task<GetDocumentDto> GetDocumentById(Guid id)
    {
        var response = await GetAsync($"v1/Documents/{id}");

        var documentResponse = await response.Content.ReadFromJsonAsync<GetDocumentDto>();

        return documentResponse ?? throw new InvalidOperationException();
    }

    public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync (GetDocumentTypesRequest request)
    {
        var response = await GetAsync("v1/DocumentTypes");
        var documentTypes = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentTypeDto>>();

        return documentTypes.Items ?? throw new InvalidOperationException();
    }
}
