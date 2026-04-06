using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public class SecurityPictureHttpClient : HttpClientBase, ISecurityPictureHttpClient
{
    public SecurityPictureHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<SecurityPictureDto>> GetAllAsync()
    {
        var response = await GetAsync("v1/SecurityPictures?size=50");
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<SecurityPictureDto>>();
        return result?.Items ?? new List<SecurityPictureDto>();
    }

    public async Task SelectPictureAsync(Guid pictureId, CreateUserSecurityPictureRequest request)
    {
        await PostAsJsonAsync($"v1/SecurityPictures/{pictureId}/select", request);
    }

    private class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
    }
}
