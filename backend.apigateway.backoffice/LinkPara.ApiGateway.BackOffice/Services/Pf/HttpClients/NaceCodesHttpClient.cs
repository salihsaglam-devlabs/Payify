using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class NaceCodesHttpClient : HttpClientBase, INaceCodesHttpClient
{
    public NaceCodesHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : 
        base(client, httpContextAccessor)
    {
    }
    
    public async Task<NaceDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/NaceCodes/{id}");
        var mcc = await response.Content.ReadFromJsonAsync<NaceDto>();
        return mcc ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<NaceDto>> GetAllAsync(GetAllNaceCodesRequest request)
    {
        var url = CreateUrlWithParams($"v1/NaceCodes", request, true);
        var response = await GetAsync(url);
        var mccList = await response.Content.ReadFromJsonAsync<PaginatedList<NaceDto>>();
        return mccList ?? throw new InvalidOperationException();
    }
}