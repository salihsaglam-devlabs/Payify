using LinkPara.ApiGateway.Services.MoneyTransfer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.MoneyTransfer
{
    public class MoneyTransferCallbacksController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpPost("GarantiPostCallbackUrl")]
        public ActionResult GarantiPostCallbackUrl(GarantiAccessTokenModel response)
        {
            return Ok(response);
        }
    }
}
