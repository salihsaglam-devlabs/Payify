using LinkPara.ApiGateway.Merchant.Services;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class AcquireBankHttpClient : HttpClientBase, IAcquireBankHttpClient
{
    public AcquireBankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<AcquireBankDto>> GetAllAsync(GetFilterAcquireBankRequest request)
    {
        var url = CreateUrlWithParams($"v1/AcquireBanks", request, true);
        var response = await GetAsync(url);
        var acquireBanks = await response.Content.ReadFromJsonAsync<PaginatedList<AcquireBankDto>>();
        return acquireBanks ?? throw new InvalidOperationException();
    }

    public async Task<AcquireBankDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/AcquireBanks/{id}");
        var acquireBank = await response.Content.ReadFromJsonAsync<AcquireBankDto>();
        return acquireBank ?? throw new InvalidOperationException();
    }
}
