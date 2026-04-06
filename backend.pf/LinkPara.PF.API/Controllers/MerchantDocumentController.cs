using LinkPara.PF.Application.Features.MerchantNotification.Queries.GetMerchantNotificationTemplate;
using LinkPara.PF.Application.Features.MerchantNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantDocuments.Queries.GetMerchantDocumentsByTransactionIdQuery;
using LinkPara.PF.Application.Features.MerchantDocuments.Commands.SaveMerchantDocument;

namespace LinkPara.PF.API.Controllers
{
    public class MerchantDocumentController : ApiControllerBase
    {
        /// <summary>
        /// Returns merchant documents by transaction id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantTransaction:Read")]
        [HttpGet("transaction/{id}")]
        public async Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId([FromRoute] Guid id)
        {
            return await Mediator.Send(new GetMerchantDocumentsByTransactionIdQuery() { Id = id });
        }

        /// <summary>
        /// Saves merchant documents by transaction id.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantTransaction:Read")]
        [HttpPost]
        public async Task SaveMerchantDocumentsByTransactionId([FromBody] SaveMerchantDocumentsByTransactionIdCommand command)
        {
            await Mediator.Send(command);
        }
    }
}
