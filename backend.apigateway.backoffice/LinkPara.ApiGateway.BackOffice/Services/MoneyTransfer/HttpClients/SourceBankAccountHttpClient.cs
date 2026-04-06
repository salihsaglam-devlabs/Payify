using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class SourceBankAccountHttpClient : HttpClientBase, ISourceBankAccountHttpClient
{
    public SourceBankAccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<SourceBankAccountDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/SourceBankAccounts/{id}");
        var account = await response.Content.ReadFromJsonAsync<SourceBankAccountDto>();
        if (!CanSeeSensitiveData())
        {
            account.IBANNumber = SensitiveDataHelper.MaskSensitiveData("Iban", account.IBANNumber);
        }
        return account;
    }

    public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request)
    {
        var url = CreateUrlWithParams($"v1/SourceBankAccounts", request, true);
        var response = await GetAsync(url);
        var accountList = await response.Content.ReadFromJsonAsync<PaginatedList<SourceBankAccountDto>>();
        if (!CanSeeSensitiveData())
        {
            accountList.Items.ForEach(s =>
            {
                s.IBANNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.IBANNumber);
            });
        }
        return accountList;
    }

    public async Task SaveAsync(SaveSourceBankAccountRequest request)
    {
        await PostAsJsonAsync($"v1/SourceBankAccounts", request);
    }

    public async Task UpdateAsync(UpdateSourceBankAccountRequest request)
    {
        await PutAsJsonAsync($"v1/SourceBankAccounts", request);
    }

    public async Task DeleteAsync(DeleteSourceBankAccountRequest request)
    {
        await DeleteAsync($"v1/SourceBankAccounts/{request.Id}");
    }

    public async Task<List<BankModel>> GetAccountBanksAsync(GetAccountBanksRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/SourceBankAccounts/account-banks" + queryString);
        return await response.Content.ReadFromJsonAsync<List<BankModel>>();
    }
}
