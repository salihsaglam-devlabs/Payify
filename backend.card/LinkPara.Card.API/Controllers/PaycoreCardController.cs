using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardSensitiveData;
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
        [Authorize(Policy = "PaycoreCard:Create")]
        [HttpPost("")]
        public async Task<CreateCardResponse> CreateCardAsync(CreateCardCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates Card Status
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Update")]
        [HttpPut("status")]
        public async Task<PaycoreResponse> UpdateCardStatusAsync(UpdateCardStatusCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Card Authorizations
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Read")]
        [HttpGet("card-authorizations")]
        public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync([FromQuery] GetCardAuthorizationsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets Card Informations
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Read")]
        [HttpGet("card-info")]
        public async Task<GetCardInformationsResponse> GetCardInformationsAsync([FromQuery]GetCardInformationsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Updates Card Authorizations
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Update")]
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
        [Authorize(Policy = "PaycoreCard:ReadAll")]
        [HttpGet("transactions")]
        public async Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets Card Last Courier Activity
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Read")]
        [HttpGet("card-last-courier-activity")]
        public async Task<GetCardLastCourierActivityResponse> GetCardLastCourierActivityAsync([FromQuery] GetCardLastCourierActivityQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Adds Additional Limit Restriction To Card
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Create")]
        [HttpPut("additional-limit-restriction")]
        public async Task<PaycoreResponse> AddAdditionalLimitRestriction(AddAdditionalLimitRestrictionCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Card Sensitive Data
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Read")]
        [HttpGet("card-sensitive-data")]
        public async Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync([FromQuery] GetCardSensitiveDataQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Renews Card
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Create")]
        [HttpPost("card-renewal")]
        public async Task<PaycoreResponse> CardRenewal(CardRenewalCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Card Status
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreCard:Read")]
        [HttpGet("card-status")]
        public async Task<GetCardStatusResponse> GetCardStatusAsync([FromQuery] GetCardStatusQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}
