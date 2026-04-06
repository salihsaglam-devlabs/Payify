using LinkPara.ApiGateway.Services.Cashback.Models.Enums;
using LinkPara.ApiGateway.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.Services.Cashback.Models.Responses;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Cashback.HttpClients;

public class CashbackHttpClient : HttpClientBase, ICashbackHttpClient
{
    private readonly IEmoneyAccountHttpClient _emoneyAccountHttpClient;
    public CashbackHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IEmoneyAccountHttpClient emoneyAccountHttpClient) : base(client, httpContextAccessor)
    {
        _emoneyAccountHttpClient = emoneyAccountHttpClient;
    }

    public async Task<PaginatedList<CashbackRuleSummaryDto>> GetFilteredRulesAsync(GetFilteredRulesRequest request)
    {
        var url = CreateUrlWithParams($"v1/Cashback", request, true);
        var response = await GetAsync(url);
        var cashbackRules = await response.Content.ReadFromJsonAsync<PaginatedList<CashbackRuleSummaryDto>>();

        foreach (var rule in cashbackRules.Items)
        {
            if (rule.ProcessType == CashbackProcessType.P2PToCorporateWallet && !string.IsNullOrEmpty(rule.CorporateWalletNumber))
            {
                rule.CorporateWalletName = (await _emoneyAccountHttpClient.GetAccountByWalletNumberAsync(rule.CorporateWalletNumber)).Name;
            }
        }
        return cashbackRules;
    }

}
