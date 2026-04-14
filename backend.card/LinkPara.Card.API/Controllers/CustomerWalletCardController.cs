using LinkPara.Card.Application.Commons.Models.WalletModels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Commands.UpdateCustomerWalletCardName;
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
        [Authorize(Policy = "CustomerWalletCard:ReadAll")]
        [HttpGet("customer-cards")]
        public async Task<List<CustomerWalletCardDto>> GetCustomerWalletCardsAsync([FromQuery] GetCustomerWalletCardsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Updates Customer Wallet Card Name
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "CustomerWalletCard:Update")]
        [HttpPut("customer-cards/name")]
        public async Task UpdateCustomerWalletCardNameAsync([FromBody] UpdateCustomerWalletCardNameCommand command)
        {
            await Mediator.Send(command);
        }
    }
}
