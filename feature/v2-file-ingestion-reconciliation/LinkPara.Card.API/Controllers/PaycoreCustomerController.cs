using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.CreateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerAddress;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerCommunication;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerLimit;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerCards;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerLimitInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class PaycoreCustomerController : ApiControllerBase
    {
        /// <summary>
        /// Creates Customer
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<PaycoreResponse> CreateCustomerAsync(CreateCustomerCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Gets Customer Informations
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<GetCustomerInformationResponse> GetCustomerInformationAsync(GetCustomerInformationQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets Customer Cards
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("cards")]
        public async Task<List<GetCustomerCardsResponse>> GetCustomerCardsAsync(GetCustomerCardsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Gets Customer Limit Info
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("limit-info")]
        public async Task<List<GetCustomerLimitInfoResponse>> GetCustomerLimitInfoAsync(GetCustomerLimitInfoQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Updates Customer
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("customer")]
        public async Task<PaycoreResponse> UpdateCustomerAsync(UpdateCustomerCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates Customer Communication
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("communication")]
        public async Task<PaycoreResponse> UpdateCustomerCommunicationAsync(UpdateCustomerCommunicationCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates Customer Address
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("address")]
        public async Task<PaycoreResponse> UpdateCustomerAddressAsync(UpdateCustomerAddressCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates Customer Limit
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("limit")]
        public async Task<PaycoreResponse> UpdateCustomerLimitAsync(UpdateCustomerLimitCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
