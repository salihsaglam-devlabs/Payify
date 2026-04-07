using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class PaycoreCardController : ApiControllerBase
    {
        /// <summary>
        /// Creates Card
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<PaycoreResponse> CreateCardAsync(CreateCardCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates Card Status
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("status")]
        public async Task<UpdateCardStatusResponse> UpdateCardStatusAsync(UpdateCardStatusCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Card Authorizations
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("card-authorizations")]
        public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets Card Informations
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("card-info")]
        public async Task<GetCardInformationsResponse> GetCardInformationsAsync(GetCardInformationsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Updates Card Authorizations
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("card-authorization")]
        public async Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Card Transactions
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("transactions")]
        public async Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}
