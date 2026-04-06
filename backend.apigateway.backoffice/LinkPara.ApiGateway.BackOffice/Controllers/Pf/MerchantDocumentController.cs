using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantDocumentController : ApiControllerBase
    {
        private readonly IMerchantDocumentHttpClient _merchantDocumentHttpClient;

        public MerchantDocumentController(IMerchantDocumentHttpClient merchantDocumentHttpClient)
        {
            _merchantDocumentHttpClient = merchantDocumentHttpClient;
        }
        /// <summary>
        /// Returns merchant documents by transaction id.
        /// </summary>
        /// <param name="merchantTransactionId"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantTransaction:Read")]
        [HttpGet("transaction/{id}")]
        public async Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId([FromRoute] Guid id)
        {
            return await _merchantDocumentHttpClient.GetMerchantDocumentsByTransactionId(id);
        }

        /// <summary>
        /// Saves merchant documents by transaction id.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantTransaction:Read")]
        [HttpPost]
        public async Task SaveMerchantDocumentsByTransactionId([FromBody] SaveMerchantDocumentsByTransactionIdRequest request)
        {
            await _merchantDocumentHttpClient.SaveMerchantDocumentsByTransactionId(request);
        }
    }
}
