using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models;
using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Request;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.HttpClients;

public class BTransDocumentClient : HttpClientBase, IBTransDocumentClient
{
    public BTransDocumentClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {

    }
    
    public async Task<PaginatedList<BtransDocumentDto>> GetAllDocumentsAsync(GetDocumentsRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Documents", request, true);

        var response = await GetAsync(url);

        var documents = await response.Content.ReadFromJsonAsync<PaginatedList<BtransDocumentDto>>();

        return documents ?? throw new InvalidOperationException();
    }

    public async Task<ParquetContentDto> GetDocumentContentAsync(Guid id)
    {
        var response = await GetAsync($"v1/Documents/{id}/get-content");
        var result = await response.Content.ReadFromJsonAsync<ParquetContentDto>();
        return result;
    }
}