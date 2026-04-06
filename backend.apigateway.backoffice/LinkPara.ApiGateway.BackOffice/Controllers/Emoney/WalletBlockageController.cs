using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney
{
    public class WalletBlockageController : ApiControllerBase
    {
        private readonly IWalletBlockageHttpClient _walletBlockageHttpClient;
        public WalletBlockageController(IWalletBlockageHttpClient walletBlockageHttpClient)
        {
            _walletBlockageHttpClient = walletBlockageHttpClient;
        }

        [Authorize(Policy = "WalletBlockage:ReadAll")]
        [HttpGet("get-blockages")]
        public async Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync([FromQuery] GetWalletBlockageRequest request)
        {
            return await _walletBlockageHttpClient.GetWalletBlockageAsync(request);
        }

        [Authorize(Policy = "WalletBlockage:Create")]
        [HttpPost("wallet-blockage")]
        public async Task WalletBlockageRequestAsync([FromBody] AddWalletBlockageRequest request)
        {
            await _walletBlockageHttpClient.AddWalletBlockageAsync(request);
        }

        [Authorize(Policy = "WalletBlockage:Create")]
        [HttpPost("add-document")]
        public async Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync([Required] IFormFile file, [Required] Guid documentTypeId, [Required] Guid walletId, string documentDescription)
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var request = new AddWalletBlockageDocumentRequest
            {
                Bytes = memoryStream.ToArray(),
                ContentType = file.ContentType,
                OriginalFileName = file.FileName,
                WalletId = walletId,
                DocumentDescription = documentDescription,
                DocumentTypeId = documentTypeId
            };

            return await _walletBlockageHttpClient.AddWalletBlockageDocumentAsync(request);
        }

        [Authorize(Policy = "WalletBlockage:Update")]
        [HttpPut("remove-document")]
        public async Task RemoveWalletBlockageDocumentAsync([FromBody] RemoveWalletBlockageDocumentRequest request)
        {
            await _walletBlockageHttpClient.RemoveWalletBlockageDocumentAsync(request);
        }

        [Authorize(Policy = "WalletBlockage:ReadAll")]
        [HttpGet("get-documents")]
        public async Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync([FromQuery] GetWalletBlockageDocumentRequest request)
        {
            return await _walletBlockageHttpClient.GetWalletBlockageDocumentsAsync(request);
        }
    }
}
