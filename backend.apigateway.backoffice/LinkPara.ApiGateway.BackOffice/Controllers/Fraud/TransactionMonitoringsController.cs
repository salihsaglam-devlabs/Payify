using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LinkPara.ApiGateway.BackOffice.Controllers.Fraud
{
    public class TransactionMonitoringsController : ApiControllerBase
    {
        private readonly ITransactionMonitoringsClient _transactionMonitoringsClient;
        public TransactionMonitoringsController(ITransactionMonitoringsClient transactionMonitoringsClient) 
        {
            _transactionMonitoringsClient = transactionMonitoringsClient;
        }
        /// <summary>
        /// Returns transaction monitoring logs
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = "FraudTransactionMonitorings:ReadAll")]
        public async Task<ActionResult<PaginatedList<TransactionMonitoringResponse>>> GetFilterAsync([FromQuery] GetAllTransactionMonitoringRequest request)
        {
            return await _transactionMonitoringsClient.GetAllAsync(request);
        }
    }
}
