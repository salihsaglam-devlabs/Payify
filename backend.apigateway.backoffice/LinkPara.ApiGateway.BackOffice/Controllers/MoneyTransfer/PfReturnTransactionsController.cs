using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer
{
    public class PfReturnTransactionsController : ApiControllerBase
    {
        private readonly IPfReturnTransactionsHttpClient _httpClient;
        public PfReturnTransactionsController(IPfReturnTransactionsHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        /// <summary>
        /// Returns pf return transaction list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = "PfReturnTransaction:ReadAll")]
        public async Task<PaginatedList<PfReturnTransactionDto>> GetListAsync([FromQuery] GetPfReturnTransactionsRequest request)
        {
            return await _httpClient.GetListAsync(request);
        }

        /// <summary>
        /// Returns pf transaction.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("return")]
        [Authorize(Policy = "PfReturnTransaction:Create")]
        public async Task<bool> ReturnTransactionAsync(ReturnPfTransactionRequest request)
        {
            return await _httpClient.ReturnPfTransactionAsync(request);
        }
    }
}
