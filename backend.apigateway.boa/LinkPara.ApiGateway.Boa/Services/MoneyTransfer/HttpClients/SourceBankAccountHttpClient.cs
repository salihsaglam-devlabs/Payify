using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.MoneyTransfer.HttpClients;

public class SourceBankAccountHttpClient : HttpClientBase, ISourceBankAccountHttpClient
{
    private readonly IEmoneyBankHttpClient _bankHttpClient;

    public SourceBankAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
        IEmoneyBankHttpClient bankHttpClient) : base(client, httpContextAccessor)
    {
        _bankHttpClient = bankHttpClient;
    }

    public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request)
    {
        var clientRequest = new GetSourceBankAccountListServiceRequest
        {
            AccountType = SharedModels.Banking.Enums.BankAccountType.UsageAccount,
            BankCode = request.BankCode,
            CurrencyCode = request.CurrencyCode,
            OrderBy = request.OrderBy,
            Page = request.Page,
            Q = request.Q,
            RecordStatus = SharedModels.Persistence.RecordStatus.Active,
            Size = request.Size,
            SortBy = request.SortBy,
            Source = SharedModels.Banking.Enums.TransactionSource.Emoney
        };

        var url = CreateUrlWithParams($"v1/SourceBankAccounts", clientRequest, true);
        var response = await GetAsync(url);
        var accountList = await response.Content.ReadFromJsonAsync<PaginatedList<SourceBankAccountDto>>();

        foreach (var item in accountList.Items)
        {
            var bankLogo = await _bankHttpClient.GetBanksAsync(item.IBANNumber);
            item.BankLogo = bankLogo?.FirstOrDefault()?.LogoUrl;
        }

        return accountList;
    }
}