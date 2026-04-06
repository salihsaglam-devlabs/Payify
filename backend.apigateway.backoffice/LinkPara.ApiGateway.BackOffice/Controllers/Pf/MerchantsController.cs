using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using Microsoft.AspNetCore.Mvc;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using Microsoft.AspNetCore.JsonPatch;
using LinkPara.ApiGateway.BackOffice.Utils;
using LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantsController : ApiControllerBase
{
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly IMapper _mapper;

    public MerchantsController(IMerchantHttpClient merchantHttpClient,
        IMapper mapper)
    {
        _merchantHttpClient = merchantHttpClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns a merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Merchant:Read")]
    public async Task<ActionResult<MerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns a masked merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("masked/{id}")]
    [Authorize(Policy = "Merchant:Read")]
    public async Task<ActionResult<MerchantMaskedDto>> GetByIdMaskedAsync([FromRoute] Guid id)
    {
        return await _merchantHttpClient.GetByIdMaskedAsync(id);
    }

    /// <summary>
    /// Returns filtered merchants
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Merchant:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantDto>>> GetFilterAsync(
        [FromQuery] GetFilterMerchantRequest request)
    {
        return await _merchantHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Export merchants as Excel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("excel")]
    [Authorize(Policy = "Merchant:ReadAll")]
    public async Task<IActionResult> GetMerchantsExcelExportAsync(
        [FromQuery] GetFilterMerchantRequest request)
    {
        var response = await _merchantHttpClient.GetFilterListAsync(request);
        var excelExportModel = _mapper.Map<List<MerchantExcelExportModel>>(response.Items);
        var excel = Excel.Instance.CreateExcelDocument(excelExportModel);
        return File(excel, "application/vnd.ms-excel", $"merchants-{DateTime.Now.ToShortDateString()}.xlsx");
    }

    /// <summary>
    /// Updates a merchant
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task<MerchantResponse> UpdateAsync(UpdateMerchantRequest request)
    {
        var merchantResponse = await _merchantHttpClient.UpdateAsync(request);

        return merchantResponse;
    }

    /// <summary>
    /// Delete merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "Merchant:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _merchantHttpClient.DeleteMerchantAsync(id);
    }

    /// <summary>
    /// Performs the approval/rejection process for the merchant
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("approve")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task ApproveAsync(ApproveMerchantRequest request)
    {
        await _merchantHttpClient.ApproveAsync(request);
    }

    /// <summary>
    /// Updates merchant with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="merchantPatch"></param>
    /// <returns></returns>
    [HttpPatch("update/{id}")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<PatchMerchantRequest> merchantPatch)
    {
        if (merchantPatch.Operations.Any())
        {
            await _merchantHttpClient.PatchAsync(id, merchantPatch);
        }
    }

    /// <summary>
    /// Generate merchant api keys 
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}/generate-apiKeys")]
    [Authorize(Policy = "Merchant:Read")]
    public async Task<ActionResult<MerchantApiKeyDto>> GenerateApiKeysAsync([FromRoute] Guid merchantId)
    {
        return await _merchantHttpClient.GenerateApiKeysAsync(merchantId);
    }

    /// <summary>
    ///  Updates merchant api keys with patch
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="merchantApiKeyPatch"></param>
    /// <returns></returns>
    [HttpPatch("{merchantId}/apiKeys")]
    [Authorize(Policy = "Merchant:Update")]
    public async Task ApiKeyPatchAsync(Guid merchantId, [FromBody] JsonPatchDocument<MerchantApiKeyPatch> merchantApiKeyPatch)
    {
        await _merchantHttpClient.ApiKeyPatchAsync(merchantId, merchantApiKeyPatch);
    }

    [Authorize(Policy = "MerchantIksAnnulment:Create")]
    [HttpPost("{merchantId}/annulments")]
    public async Task<IKSResponse<AnnulmentResponse>> SaveAnnulmentAsync(Guid merchantId, [FromBody] SaveAnnulmentRequest request)
    {
        return await _merchantHttpClient.SaveAnnulmentAsync(merchantId, request);
    }
    
    /// <summary>
    /// Checks loss-making rates in merchant pricing setup
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPost("pricing-preview")]
    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(MerchantPricingPreviewRequest request)
    {
        return await _merchantHttpClient.PreviewPricingProfileUpdateAsync(request);
    }
}
