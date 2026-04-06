using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.CampaignManagement
{
    public class IWalletChargesController : ApiControllerBase
    {
        private readonly IChargeHttpClient _chargeHttpClient;

        public IWalletChargesController(IChargeHttpClient chargeHttpClient)
        {
            _chargeHttpClient = chargeHttpClient;
        }

        /// <summary>
        /// Charge Transactions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "IWalletCharge:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<ChargeTransactionResponse>> GetChargeTransactionsAsync([FromQuery] GetChargeTransactionsSearchRequest request)
        {
            return await _chargeHttpClient.GetChargeTransactionsAsync(request);
        }
    }
}
