using LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.IKS
{
    public class AnnulmentsController : ApiControllerBase
    {

        private readonly IAnnulmentHttpClient _annulmentHttpClient;
        public AnnulmentsController(IAnnulmentHttpClient annulmentHttpClient)
        {
            _annulmentHttpClient = annulmentHttpClient;
        }

        /// <summary>
        /// Annulment codes
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Read")]
        [HttpGet("")]
        public async Task<IKSResponse<AnnulmentCodesResponse>> AnnulmentCodesAsync()
        {
            return await _annulmentHttpClient.AnnulmentCodesAsync();
        }

        /// <summary>
        /// Annulments query
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Read")]
        [HttpGet("annulmentsQuery")]
        public async Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentsQueryAsync([FromQuery] AnnulmentsQueryRequest request)
        {
            return await _annulmentHttpClient.AnnulmentsQueryAsync(request);
        }

        /// <summary>
        /// Card Bins
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksAnnulment:Read")]
        [HttpGet("cardBins")]
        public async Task<IKSResponse<CardBinResponse>> CardBinsAsync()
        {
            return await _annulmentHttpClient.GetCardBinAsync();
        }
    }
}
