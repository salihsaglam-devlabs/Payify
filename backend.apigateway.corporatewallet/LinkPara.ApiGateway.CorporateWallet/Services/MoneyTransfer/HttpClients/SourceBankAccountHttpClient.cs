using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.HttpClients
{
    public class SourceBankAccountHttpClient : HttpClientBase, ISourceBankAccountHttpClient
    {
        private readonly IBankHttpClient _bankHttpClient;

        public SourceBankAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
            IBankHttpClient bankHttpClient)
            : base(client, httpContextAccessor)
        {
            _bankHttpClient = bankHttpClient;
        }

        public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request)
        {
            var url = CreateUrlWithParams($"v1/SourceBankAccounts", request, true);
            var response = await GetAsync(url);
            var accountList = await response.Content.ReadFromJsonAsync<PaginatedList<SourceBankAccountDto>>();

            foreach (var item in accountList.Items)
            {
                var bankLogo = await _bankHttpClient.GetBanksAsync(item.IBANNumber);
                item.Bank.LogoUrl = bankLogo?.FirstOrDefault()?.LogoUrl;
            }

            return accountList;
        }
    }
}