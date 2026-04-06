using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.HttpClients;

public class CashbackHttpClient : HttpClientBase, ICashbackHttpClient
{
    public CashbackHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<CashbackRuleSummaryDto>> GetFilteredRulesAsync(GetFilteredRulesRequest request)
    {
        var url = CreateUrlWithParams($"v1/Cashback", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<CashbackRuleSummaryDto>>();
    }

    public async Task<CashbackRuleDto> GetByIdAsync(Guid ruleId)
    {
        var response = await GetAsync($"v1/Cashback/{ruleId}");

        return await response.Content.ReadFromJsonAsync<CashbackRuleDto>();
    }

    public async Task<CreateRuleResponse> CreateRuleAsync(CreateRuleRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Cashback", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var ruleResponse = JsonSerializer.Deserialize<CreateRuleResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return ruleResponse;
    }

    public async Task UpdateRuleAsync(UpdateRuleRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Cashback", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task DeleteRuleAsync(Guid ruleId)
    {
        var response = await DeleteAsync($"v1/Cashback/{ruleId}");

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<ValidateRuleResponse> ValidateRuleAsync(ValidateRuleRequest request)
    {
        var response = await PostAsJsonAsync($"v1/Cashback/validate-rule", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var ruleResponse = JsonSerializer.Deserialize<ValidateRuleResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return ruleResponse;
    }

    public async Task<PaginatedList<CashbackEntitlementDto>> GetFilteredTransactionAsync(GetCashbackTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/Cashback/cashback-transaction", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<CashbackEntitlementDto>>();
    }
}
