using System.Net.Http.Json;
using LinkPara.HttpProviders.BTrans.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.BTrans;

public class BTransDocumentService : HttpClientBase, IBTransDocumentService
{
    public BTransDocumentService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }
    
    public async Task<PaginatedList<DocumentDto>> GetAllDocumentsAsync(GetDocumentsRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Documents", request, true);

        var response = await GetAsync(url);

        var documents = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentDto>>();

        return documents ?? throw new InvalidOperationException();
    }
    
    public async Task<ParquetContentDto> GetDocumentContentAsync(Guid id)
    {
        var response = await GetAsync($"v1/Documents/{id}/get-content");
        var result = await response.Content.ReadFromJsonAsync<ParquetContentDto>();
        return result;
    }
}