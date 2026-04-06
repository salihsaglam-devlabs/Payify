using LinkPara.ApiGateway.Card.Services.Card.HttpClients;
using LinkPara.ApiGateway.Card.Services.Card.Models;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Card.Controllers
{
    public class DebitAuthorizationController : ApiControllerBase
    {
        private readonly IDebitAuthorizationHttpClient _httpClient;
        public DebitAuthorizationController(IDebitAuthorizationHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        /// <summary>
        /// Process Debit Authorization.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<DebitAuthorizationResponse> ProcessDebitAuthorizationAsync(DebitAuthorizationRequest request)
        {
            return await _httpClient.ProcessDebitAuthorizationAsync(request);
        }
    }
}
