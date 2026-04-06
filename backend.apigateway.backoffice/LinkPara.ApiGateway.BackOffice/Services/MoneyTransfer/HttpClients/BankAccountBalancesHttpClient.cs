using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
public class BankAccountBalancesHttpClient : HttpClientBase, IBankAccountBalancesHttpClient
{
    public BankAccountBalancesHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task<PaginatedList<BankAccountBalanceDto>> GetBankAccountBalancesAsync(BankAccountBalanceRequest request)
    {
        var url = CreateUrlWithParams($"v1/BankAccountBalances", request, true);
        var response = await GetAsync(url);
        var accountList = await response.Content.ReadFromJsonAsync<PaginatedList<BankAccountBalanceDto>>();
        if (!CanSeeSensitiveData())
        {
            accountList.Items.ForEach(s =>
            {
                s.IBANNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.IBANNumber);
            });
        }
        return accountList;
    }
}
