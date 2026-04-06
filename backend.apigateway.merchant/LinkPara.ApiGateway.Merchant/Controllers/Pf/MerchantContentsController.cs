using System.ComponentModel.DataAnnotations;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantContentsController : ApiControllerBase
{
    private readonly IMerchantContentHttpClient _merchantContentHttpClient;
    private readonly IStringLocalizer _localizer;

    public MerchantContentsController(IMerchantContentHttpClient merchantContentHttpClient,
        IStringLocalizerFactory factory)
    {
        _merchantContentHttpClient = merchantContentHttpClient;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.Merchant");
    }
    
    /// <summary>
    /// Get all merchant contents.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantContentDto>> GetAllMerchantContentAsync([FromQuery] GetFilterMerchantContentRequest request)
    {
        return await _merchantContentHttpClient.GetAllMerchantContentAsync(request);
    }
    
    /// <summary>
    /// Returns merchant content by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "MerchantContent:Read")]
    public async Task<MerchantContentDto> GetMerchantContentByIdAsync([FromRoute] Guid id)
    {
        return await _merchantContentHttpClient.GetMerchantContentByIdAsync(id);
    }
    
    /// <summary>
    /// Create a merchant content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Create")]
    [HttpPost("")]
    public async Task CreateMerchantContentAsync(CreateMerchantContentRequest request)
    {
        await _merchantContentHttpClient.CreateMerchantContentAsync(request);
    }
    
    /// <summary>
    /// Updates a merchant content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Update")]
    [HttpPut("")]
    public async Task PutMerchantContentAsync(MerchantContentDto request)
    {
        await _merchantContentHttpClient.PutMerchantContentAsync(request);
    }
    
    /// <summary>
    /// Delete a merchant content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteMerchantContentAsync(Guid id)
    {
        await _merchantContentHttpClient.DeleteMerchantContentAsync(id);
    }
    
    /// <summary>
    /// Returns merchant logo by merchantId.
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Read")]
    [HttpGet("logo/{merchantId}")]
    public async Task<MerchantLogoDto> GetMerchantLogoAsync([FromRoute] Guid merchantId)
    {
        return await _merchantContentHttpClient.GetMerchantLogoAsync(merchantId);
    }
    
    /// <summary>
    /// Uploads a merchant logo.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Create")]
    [RequestSizeLimit(bytes: 4500000)]
    [HttpPost("logo")]
    public async Task UploadMerchantLogoAsync([Required] IFormFile file, [FromQuery] Guid merchantId)
    {
        string[] allowedContentTypes = { "png", "jpeg", "pdf" };
        if (!allowedContentTypes.Any(contentType => file.ContentType.Contains(contentType)))
        {
            throw new InvalidImageFormatException(_localizer.GetString("InvalidImageFormatException"));
        }
        
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var merchantLogo = new MerchantLogoDto
        {
            MerchantId = merchantId,
            ContentType = file.ContentType,
            FileName = file.FileName,
            Bytes = memoryStream.ToArray()
        };
        
        await _merchantContentHttpClient.UploadMerchantLogoAsync(merchantLogo);
    }
}