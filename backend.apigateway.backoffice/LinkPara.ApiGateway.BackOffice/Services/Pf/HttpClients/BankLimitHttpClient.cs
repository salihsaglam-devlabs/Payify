using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class BankLimitHttpClient : HttpClientBase, IBankLimitHttpClient
{
    public BankLimitHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task DeleteBankLimitAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/BankLimits/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<BankLimitDto>> GetAllAsync(GetFilterBankLimitRequest request)
    {
        var url = CreateUrlWithParams($"v1/BankLimits", request, true);
        var response = await GetAsync(url);
        var bankLimits = await response.Content.ReadFromJsonAsync<PaginatedList<BankLimitDto>>();
        return bankLimits ?? throw new InvalidOperationException();
    }

    public async Task<BankLimitDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BankLimits/{id}");
        var bankLimit = await response.Content.ReadFromJsonAsync<BankLimitDto>();
        return bankLimit ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveBankLimitRequest request)
    {
        var response = await PostAsJsonAsync($"v1/BankLimits", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateAsync(UpdateBankLimitRequest request)
    {
        var response = await PutAsJsonAsync($"v1/BankLimits", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
