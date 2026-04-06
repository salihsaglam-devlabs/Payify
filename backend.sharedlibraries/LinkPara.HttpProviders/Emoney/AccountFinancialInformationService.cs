using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.Emoney;

public class AccountFinancialInformationService : HttpClientBase, IAccountFinancialInformationService
{
    public AccountFinancialInformationService(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task CreateAccountFinancialInfoAsync(CreateAccountFinancialInfoRequest request)
    {
        var response = await PostAsJsonAsync("v1/AccountFinancialInformation", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}
