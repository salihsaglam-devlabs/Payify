using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public class UserAddressHttpClient : HttpClientBase, IUserAddressHttpClient
{

    private readonly IServiceRequestConverter _serviceRequestConverter;
    public UserAddressHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task CreateAddressAsync(CreateAddressRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<CreateAddressRequest, CreateAddressServiceRequest>(request);

        await PostAsJsonAsync($"v1/UserAddresses", clientRequest);
    }

    public async Task<List<UserAddressDto>> GetAddressByUserIdAsync(string userId)
    {
        var response = await GetAsync($"v1/UserAddresses/{userId}");

        var responseString = await response.Content.ReadAsStringAsync();

        var userAddresses = JsonSerializer.Deserialize<List<UserAddressDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userAddresses ?? throw new InvalidOperationException();
    }

    public async Task UpdateAddressAsync(UpdateAddressRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateAddressRequest, UpdateAddressServiceRequest>(request);
        await PutAsJsonAsync($"v1/UserAddresses/{request.Id}", clientRequest);
    }

}
