using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.MoneyTransferReconciliation;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer
{
    public class MoneyTransferReconciliationsController : ApiControllerBase
    {
        private readonly IMoneyTransferReconciliationHttpClient _reconciliationHttpClient;
        public MoneyTransferReconciliationsController(IMoneyTransferReconciliationHttpClient reconciliationHttpClient)
        {
            _reconciliationHttpClient = reconciliationHttpClient;
        }

        /// <summary>
        /// get money transfer reconciliation summaries
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("summaries")]
        [Authorize(Policy = "MoneyTransferReconciliationSummary:ReadAll")]
        public async Task<PaginatedList<ReconciliationSummaryDto>> GetReconciliationSummaryAsync([FromQuery] ReconciliationSummaryRequest request)
        {
            return await _reconciliationHttpClient.GetReconciliationSummaryAsync(request);
        }

        /// <summary>
        /// get money transfer reconciliation summary details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("summary/details")]
        [Authorize(Policy = "MoneyTransferReconciliationDetail:ReadAll")]
        public async Task<PaginatedList<ReconciliationDetailDto>> GetReconciliationDetailsAsync([FromQuery] ReconciliationDetailRequest request)
        {
            return await _reconciliationHttpClient.GetReconciliationDetailAsync(request);
        }

        /// <summary>
        /// ignore a reconciliation detail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>        
        [HttpPut("summary/details/{id}/cancel")]
        [Authorize(Policy = "MoneyTransferReconciliationDetail:Update")]
        public async Task CancelReconciliationDetailAsync(CancelReconciliationDetailRequest request)
        {
            await _reconciliationHttpClient.CancelReconciliationDetailAsync(request);
        }

        /// <summary>
        /// run reconciliation again
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Policy = "MoneyTransferReconciliationSummary:Create")]
        public async Task RunReconciliationAsync(RunReconciliationRequest request)
        {
            await _reconciliationHttpClient.RunReconciliationAsync(request);
        }
    }
}
