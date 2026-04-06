using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class BulkTransferHttpClient : HttpClientBase, IBulkTransferHttpClient
{
    public BulkTransferHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task ActionBulkTransferAsync(ActionBulkTransferRequest request)
    {
        await PutAsJsonAsync($"v1/BulkTransfers/action", request);
    }

    public async Task<BulkTransferDto> GetBulkTransferByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BulkTransfers/{id}");
        var bulkTransfer = await response.Content.ReadFromJsonAsync<BulkTransferDto>();
        return bulkTransfer ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<BulkTransferDto>> GetBulkTransfersAsync(GetBulkTransfersRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/BulkTransfers{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var bulkTransfers = JsonSerializer.Deserialize<PaginatedList<BulkTransferDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return bulkTransfers ?? throw new InvalidOperationException();
    }

    public async Task<BulkTransferDto> GetReportBulkTransferByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BulkTransfers/report/{id}");
        var bulkTransfer = await response.Content.ReadFromJsonAsync<BulkTransferDto>();
        return bulkTransfer ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<BulkTransferDto>> GetReportBulkTransfersAsync(GetReportBulkTransferRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/BulkTransfers/report{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var bulkTransfers = JsonSerializer.Deserialize<PaginatedList<BulkTransferDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return bulkTransfers ?? throw new InvalidOperationException();
    }

    public async Task SaveBulkTransferAsync(SaveBulkTransferRequest request)
    {
        await PostAsJsonAsync($"v1/BulkTransfers", request);
    }
}
