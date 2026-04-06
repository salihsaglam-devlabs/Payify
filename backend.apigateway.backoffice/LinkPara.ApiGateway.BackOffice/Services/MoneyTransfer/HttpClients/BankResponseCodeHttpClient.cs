using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class BankResponseCodeHttpClient : HttpClientBase, IBankResponseCodeHttpClient
{
    public BankResponseCodeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BankResponseCodeDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BankResponseCodes/{id}");
        var responseCode = await response.Content.ReadFromJsonAsync<BankResponseCodeDto>();
        return responseCode;
    }

    public async Task<PaginatedList<BankResponseCodeDto>> GetListAsync(GetBankResponseCodeListRequest request)
    {
        var url = CreateUrlWithParams($"v1/BankResponseCodes", request, true);
        var response = await GetAsync(url);
        var responseCodeList = await response.Content.ReadFromJsonAsync<PaginatedList<BankResponseCodeDto>>();
        return responseCodeList;
    }

    public async Task SaveAsync(SaveBankResponseCodeRequest request)
    {
        await PostAsJsonAsync($"v1/BankResponseCodes", request);
    }

    public async Task UpdateAsync(UpdateBankResponseCodeRequest request)
    {
        await PutAsJsonAsync($"v1/BankResponseCodes", request);
    }

    public async Task DeleteAsync(DeleteBankResponseCodeRequest request)
    {
        await DeleteAsync($"v1/BankResponseCodes/{request.Id}");
    }
}
