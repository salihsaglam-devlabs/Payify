using LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels;
using LinkPara.Card.Application.Features.PaycoreServices.DebitAuthorizationServices.Commands.DebitAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class PaycoreDebitAuthorizationController : ApiControllerBase
    {
        /// <summary>
        /// Authorization Service for Transactions
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("debit-auth")]
        public async Task<DebitAuthorizationResponse> DebitAuthorizationAsync([FromBody] DebitAuthorizationCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
