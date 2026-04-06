using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.PF;

public class BankAccountService : HttpClientBase, IBankAccountService
{
    public BankAccountService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }
    public async Task<MerchantBankAccountDto> GetBankAccountByMerchantId(GetBankAccountRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/BankAccounts", request, true);

        var response = await GetAsync(url);

        var bankAccountDto = await response.Content.ReadFromJsonAsync<MerchantBankAccountDto>();

        return bankAccountDto ?? throw new InvalidOperationException();
    }
}
