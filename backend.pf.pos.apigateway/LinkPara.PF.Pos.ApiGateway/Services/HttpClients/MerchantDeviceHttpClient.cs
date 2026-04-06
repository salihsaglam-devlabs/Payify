using System.Text.Json;
using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using LinkPara.SystemUser;

namespace LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

public class MerchantDeviceHttpClient : HttpClientBase, IMerchantDeviceHttpClient
{
    public MerchantDeviceHttpClient(HttpClient client, IApplicationUserService applicationUserService) 
        : base(client , applicationUserService)
    {
    }

    public async Task<MerchantDeviceApiKeyDto> GetDeviceApiKeysAsync(string publicKey)
    {
        var publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(publicKey);
        var publicKeyEncoded = Convert.ToBase64String(publicKeyBytes);
        
        var response = await GetAsync($"v1/MerchantDevices/apiKeys?PublicKeyEncoded={publicKeyEncoded}");
        var responseString = await response.Content.ReadAsStringAsync();
        var apiKeys = JsonSerializer.Deserialize<MerchantDeviceApiKeyDto>(responseString, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return apiKeys;
    }
}