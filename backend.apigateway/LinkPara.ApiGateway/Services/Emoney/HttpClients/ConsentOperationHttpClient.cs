using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;
public class ConsentOperationHttpClient : HttpClientBase, IConsentOperationHttpClient
{
    public ConsentOperationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<List<ConsentDto>> GetActiveConsentListAsync(GetActiveConsentListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/ConsentOperations/active-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ConsentDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;

    }

    public async Task<CancelConsentResultDto> CancelConsentAsync(CancelConsentRequest request)
    {
        var result = await PostAsJsonAsync($"v1/ConsentOperations/cancel-consent", request);
        var response = await result.Content.ReadFromJsonAsync<CancelConsentResultDto>();
        return response;

    }

    public async Task<GetWaitingApprovalConsentResponse> GetWaitingApprovalConsentsAsync(GetWaitingApprovalConsentRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/ConsentOperations/waiting-approval-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<WaitingApprovalConsentDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return new GetWaitingApprovalConsentResponse { Value = result };
    }

    public async Task<GetConsentDetailResponse> GetConsentDetailAsync(GetConsentDetailRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/ConsentOperations/consent-detail" + queryString);

        return await response.Content.ReadFromJsonAsync<GetConsentDetailResponse>();
    }

    public async Task<UpdateConsentResultDto> UpdateConsentAsync(UpdateConsentRequest request)
    {
        var result = await PostAsJsonAsync($"v1/ConsentOperations/update-consent", request);
        var response = await result.Content.ReadFromJsonAsync<UpdateConsentResultDto>();
        return response;

    }

}