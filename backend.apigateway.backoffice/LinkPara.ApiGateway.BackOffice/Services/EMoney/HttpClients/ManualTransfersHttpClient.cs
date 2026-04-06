using System.Net.Http.Headers;
using System.Reflection;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class ManualTransfersHttpClient : HttpClientBase, IManualTransfersHttpClient
{
    public ManualTransfersHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<ManualTransferResponse>> GetAllManualTransfersAsync(GetAllManualTransfersRequest request)
    {
        var url = CreateUrlWithParams($"v1/ManualTransfers", request, true);
        var response = await GetAsync(url);
        
        var result =  await response.Content.ReadFromJsonAsync<PaginatedList<ManualTransferResponse>>();
        
        return result ?? throw new InvalidOperationException();
    }

    public async Task CreateManualTransfersAsync(CreateManualTransferRequest request)
    {
        var response = await PostAsJsonAsync($"v1/ManualTransfers", request);
        
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}