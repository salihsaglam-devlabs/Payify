using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney
{
    public class ChargebackController : ApiControllerBase
    {        
        private readonly IChargebackHttpClient _chargebackHttpClient;        

        public ChargebackController(IChargebackHttpClient chargebackHttpClient)
        {
            _chargebackHttpClient = chargebackHttpClient;
        }

        /// <summary>
        /// Gets and Filters Chargeback Items
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "Chargeback:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<ChargebackDto>> GetChargebackAsync([FromQuery] GetChargebackRequest request)
        {
            return await _chargebackHttpClient.GetChargebackAsync(request);
        }

        /// <summary>
        /// Initialize Chargeback Operation
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Chargeback:Create")]
        [HttpPost("init")]
        public async Task<ChargebackDto> InitializeChargebackAsync([FromBody] InitChargebackRequest request)
        {
            return await _chargebackHttpClient.InitializeChargebackAsync(request);
        }

        /// <summary>
        /// Approve Chargeback Operation
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Chargeback:Update")]
        [HttpPut("approve")]
        public async Task<ChargebackDto> ApproveChargebackAsync([FromBody] ApproveChargebackRequest request)
        {
            return await _chargebackHttpClient.ApproveChargebackAsync(request);
        }

        /// <summary>
        /// Creates Chargeback Document
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Chargeback:Create")]
        [HttpPost("add-document")]
        public async Task<ChargebackDocumentDto> AddChargebackDocumentAsync([Required] IFormFile file, [Required] Guid documentTypeId, [Required] Guid chargebackId, string documentDescription)
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var request = new AddChargebackDocumentRequest
            {
                Bytes = memoryStream.ToArray(),
                ContentType = file.ContentType,
                OriginalFileName = file.FileName,
                ChargebackId = chargebackId,
                DocumentDescription = documentDescription,
                DocumentTypeId = documentTypeId
            };

            return await _chargebackHttpClient.AddChargebackDocumentAsync(request);
        }

        /// <summary>
        /// Deletes Chargeback Document
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Chargeback:Update")]
        [HttpPut("delete-document")]
        public async Task<bool> DeleteChargebackDocumentAsync([FromBody] DeleteChargebackDocumentRequest request)
        {
            return await _chargebackHttpClient.DeleteChargebackDocumentAsync(request);
        }

        /// <summary>
        /// Gets Chargeback Documents
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Chargeback:ReadAll")]
        [HttpGet("get-documents")]
        public async Task<List<ChargebackDocumentDto>> GetChargebackDocumentAsync([FromQuery] GetChargebackDocumentRequest request)
        {
            return await _chargebackHttpClient.GetChargebackDocumentsAsync(request);
        }
    }
}
