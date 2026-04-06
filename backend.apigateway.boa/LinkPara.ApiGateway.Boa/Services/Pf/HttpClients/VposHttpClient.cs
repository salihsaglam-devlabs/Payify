using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public class VposHttpClient : HttpClientBase, IVposHttpClient
{
    public VposHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }
    
    public async Task<PaginatedList<VposDto>> GetFilterListAsync(GetFilterVposRequest request)
    {
        var url = CreateUrlWithParams($"v1/BoaVpos", request, true);
        var response = await GetAsync(url);
        var vpos = await response.Content.ReadFromJsonAsync<PaginatedList<VposDto>>();
        return vpos ?? throw new InvalidOperationException();
    }
}