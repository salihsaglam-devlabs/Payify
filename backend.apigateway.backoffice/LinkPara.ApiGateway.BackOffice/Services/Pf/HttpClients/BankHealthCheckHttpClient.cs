using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class BankHealthCheckHttpClient : HttpClientBase, IBankHealthCheckHttpClient
{
    public BankHealthCheckHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<BankHealthCheckDto>> GetAllAsync(GetFilterBankHealthCheckRequest request)
    {
        var url = CreateUrlWithParams($"v1/BankHealthChecks", request, true);
        var response = await GetAsync(url);
        var bankHealthChecks = await response.Content.ReadFromJsonAsync<PaginatedList<BankHealthCheckDto>>();
        return bankHealthChecks ?? throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateBankHealthCheckRequest request)
    {
        var response = await PutAsJsonAsync($"v1/BankHealthChecks", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
