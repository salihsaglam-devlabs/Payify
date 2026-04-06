using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public class MccHttpClient : HttpClientBase, IMccHttpClient
{
    public MccHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : 
        base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<MccDto>> GetAllAsync(GetFilterMccRequest request)
    {
        var url = CreateUrlWithParams($"v1/BoaMcc", request, true);
        var response = await GetAsync(url);
        var mccList = await response.Content.ReadFromJsonAsync<PaginatedList<MccDto>>();
        return mccList ?? throw new InvalidOperationException();
    }
}