using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney
{
    public class CurrenciesController : ApiControllerBase
    {
        private readonly ICurrencyHttpClient _currencyHttpClient;

        public CurrenciesController(ICurrencyHttpClient currencyHttpClient)
        {
            _currencyHttpClient = currencyHttpClient;
        }

        /// <summary>
        /// Returns all currencies list
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "EmoneyCurrency:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<List<CurrencyDto>>> GetAllAsync([FromQuery] CurrenciesFilterRequest request)
        {
            return await _currencyHttpClient.GetAllAsync(request);
        }
    }
}
