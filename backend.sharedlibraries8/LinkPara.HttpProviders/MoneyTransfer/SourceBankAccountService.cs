using System.Net.Http.Json;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.MoneyTransfer;

public class SourceBankAccountService : HttpClientBase, ISourceBankAccountService
{
    public SourceBankAccountService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<SourceBankAccountDto>> GetAllSourceBankAccountsAsync(GetSourceBankAccountsRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/SourceBankAccounts", request, true);

        var response = await GetAsync(url);

        var bankAccounts = await response.Content.ReadFromJsonAsync<PaginatedList<SourceBankAccountDto>>();

        return bankAccounts ?? throw new InvalidOperationException();
    }
}