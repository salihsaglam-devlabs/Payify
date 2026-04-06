using LinkPara.Card.Application.Commons.Models.WalletMoodels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class CustomerWalletCardController : ApiControllerBase
    {
        /// <summary>
        /// Gets Customer Cards
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("customer-cards")]
        public async Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync([FromQuery] GetCustomerWalletCardsQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}
