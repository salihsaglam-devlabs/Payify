using LinkPara.Emoney.ApiGateway.Commons.Extensions;
using LinkPara.Emoney.ApiGateway.Models.Requests;
using LinkPara.Emoney.ApiGateway.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.ApiGateway.Services.HttpClients;

public class ProvisionHttpClient : HttpClientBase, IProvisionHttpClient
{
    private readonly Guid _partnerId;
    public ProvisionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client)
    {
        var headers = httpContextAccessor.HttpContext!.Request.Headers;

        if (headers.ContainsKey("partnerId"))
        {
            _partnerId = Guid.Parse(headers["partnerId"]);
        }
    }

    public async Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Provisions", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }

    public async Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionRequest request)
    {
        if (_partnerId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/Provisions/inquire{queryString}");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var result = await response.Content.ReadFromJsonAsync<InquireProvisionResponse>();

        return result ?? throw new InvalidOperationException();
    }

    public async Task<ProvisionResponse> ProvisionAsync(ProvisionRequest request)
    {
        if (_partnerId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var serviceRequest = new ProvisionServiceRequest(request, _partnerId);
        var response = await PostAsJsonAsync("v1/Provisions", serviceRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<ProvisionResponse>();

        return provisionResponse ?? throw new InvalidCastException();
    }
}
