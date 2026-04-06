using LinkPara.PF.Application.Features.MerchantDevices.Queries;
using LinkPara.PF.Application.Features.MerchantDevices.Queries.GetMerchantDeviceApiKeys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantDevicesController : ApiControllerBase
{
    /// <summary>
    /// Get public/private key pair of merchant device
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("apiKeys")]
    public async Task<MerchantDeviceApiKeyDto> GetApiKeysAsync([FromQuery] GetMerchantDeviceApiKeysQuery query)
    {
        return await Mediator.Send(query);
    }
}