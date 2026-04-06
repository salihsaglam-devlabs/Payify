using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class AccountFinancialInfoHttpClient : HttpClientBase, IAccountFinancialInfoHttpClient
{
    public AccountFinancialInfoHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<AccountFinancialInfoDto> GetAccountFinancialInfoAsync(Guid accountId)
    {
        var response = await GetAsync($"v1/AccountFinancialInformation/{accountId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new NotFoundException(nameof(AccountFinancialInfoDto), accountId);
        }

        var result = await response.Content.ReadFromJsonAsync<AccountFinancialInfoDto>();

        return result ?? throw new InvalidOperationException();
    }
}
