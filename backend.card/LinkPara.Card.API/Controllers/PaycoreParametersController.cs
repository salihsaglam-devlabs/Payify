using LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels;
using LinkPara.Card.Application.Features.PaycoreServices.ParameterServices.Queries.GetProductsQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers
{
    public class PaycoreParametersController : ApiControllerBase
    {
        /// <summary>
        /// Return Paycore Product List
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "PaycoreParameters:ReadAll")]
        [HttpGet()]
        public async Task<GetProductsResponse> GetProductsAsync()
        {
            return await Mediator.Send(new GetProductsQuery());
        }
    }
}
