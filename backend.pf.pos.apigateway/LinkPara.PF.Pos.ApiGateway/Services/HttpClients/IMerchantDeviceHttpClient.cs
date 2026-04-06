using LinkPara.PF.Pos.ApiGateway.Models.Responses;

namespace LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

public interface IMerchantDeviceHttpClient
{
    Task<MerchantDeviceApiKeyDto> GetDeviceApiKeysAsync(string publicKey);
}