using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class PaycoreCardPinController : ApiControllerBase
    {
        /// <summary>
        /// Encryption Card Pin
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("set-card-pin")]
        public async Task<PaycoreResponse> SetCardPinAsync(SetCardPinCommand command)
        {
            return await Mediator.Send(command);
        }

    }
}
