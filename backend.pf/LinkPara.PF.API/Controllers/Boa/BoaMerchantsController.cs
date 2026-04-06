using LinkPara.PF.Application.Features.Boa.Merchants;
using LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;
using LinkPara.PF.Application.Features.Boa.Merchants.Queries.GetBoaMerchantByMerchantNumber;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.Boa;

public class BoaMerchantsController : ApiControllerBase
{
    /// <summary>
    /// Creates a new merchant for the boa system.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task<ActionResult<CreateBoaMerchantResponse>> CreateBoaMerchantAsync(CreateBoaMerchantCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Returns merchant for the boa system.
    /// </summary>
    /// <param name="merchantNumber"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{merchantNumber}")]
    public async Task<ActionResult<BoaMerchantDto>> GetBoaMerchantAsync([FromRoute] string merchantNumber)
    {
        return await Mediator.Send(new GetBoaMerchantByMerchantNumberQuery { MerchantNumber = merchantNumber });
    }
}