using LinkPara.ApiGateway.Boa.Commons.Extensions;
using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;

public class AccountHttpClient : HttpClientBase, IAccountHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public AccountHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter) : base(client, httpContextAccessor)

    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetEmailUpdateTokenRequest, GetEmailUpdateTokenServiceRequest>(request);

        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/account/update-email/token" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GetEmailUpdateTokenResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task UpdateEmailAsync(UpdateEmailRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateEmailRequest, UpdateEmailServiceRequest>(request);

        await PostAsJsonAsync($"v1/account/update-email", clientRequest);
    }

    public async Task<GetPhoneNumberTokenResponse> GetPhoneNumberUpdateTokenAsync(GetPhoneNumberUpdateTokenRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetPhoneNumberUpdateTokenRequest, GetPhoneNumberUpdateTokenServiceRequest>(request);

        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/account/update-phone/token" + queryString);

        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GetPhoneNumberTokenResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdatePhoneNumberRequest, UpdatePhoneNumberServiceRequest>(request);

        await PostAsJsonAsync($"v1/account/update-phone-number", clientRequest);
    }

    public async Task<CreateIndividualCustomerResponse> CreateIndividualCustomerAsync(CreateIndividualCustomerRequestWithUsername request)
    {
        var response = await PostAsJsonAsync("v1/account/register-with-customer", request);

        return await response.Content.ReadFromJsonAsync<CreateIndividualCustomerResponse>();
    }
}