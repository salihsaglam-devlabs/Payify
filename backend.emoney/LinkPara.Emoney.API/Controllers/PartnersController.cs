using LinkPara.Emoney.Application.Features.Partners.Commands.CreatePartner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.Partners.Queries.GetApiKey;
using LinkPara.Emoney.Application.Features.Partners;

namespace LinkPara.Emoney.API.Controllers;

public class PartnersController : ApiControllerBase
{
    /// <summary>
    /// Get ApiKey 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Partner:Read")]
    [HttpGet("api-keys")]
    public async Task<ApiKeyDto> GetApiKeyAsync([FromQuery] GetApiKeyQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Create a partner
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Partner:Create")]
    [HttpPost("")]
    public async Task CreatePartnerAsync(CreatePartnerCommand command)
    {
        await Mediator.Send(command);
    }
}
