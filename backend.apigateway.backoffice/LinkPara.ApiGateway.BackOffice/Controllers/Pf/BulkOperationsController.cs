using System.ComponentModel.DataAnnotations;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class BulkOperationsController : ApiControllerBase
{
    private const string AccessKey = "q9ZP3JcW8mKkR0XyBfL5D2S8aYHnE1TQ4M6VbUoNw3I=";
    private readonly IBulkOperationsHttpClient _bulkOperationsHttpClient;
    private readonly IStringLocalizer _localizer;
    
    public BulkOperationsController(IBulkOperationsHttpClient bulkOperationsHttpClient, IStringLocalizerFactory factory)
    {
        _bulkOperationsHttpClient = bulkOperationsHttpClient;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.BackOffice");
    }

    /// <summary>
    /// Pre-validates a bulk merchant excel file to ensure its file type is valid and processes the file for validation checks
    /// including its metadata before bulk import begins.
    /// </summary>
    /// <param name="file">The excel file to be validated. Only files with ".xlsx" extensions are allowed.</param>
    /// <param name="bulkMerchantInsertRequest">Metadata or configurations required for the pre-validation process, including access keys and merchant properties.</param>
    /// <returns>A <see cref="BulkMerchantExcelValidationResponse"/> object that contains the result of the pre-validation operation.</returns>
    /// <exception cref="ForbiddenAccessException">Thrown when the access key in the request does not match the expected key.</exception>
    /// <exception cref="InvalidFileExtensionException">Thrown when the file does not have a valid ".xlsx" extension.</exception>
    // [Authorize(Policy = "PfBulkOperations:Create")]
    [AllowAnonymous]
    [HttpPost("prevalidate-bulk-merchant")]
    [Consumes("multipart/form-data")]
    public async Task<BulkMerchantExcelValidationResponse> PreValidateBulkMerchantExcelFileAsync(
        [Required] IFormFile file, 
        [FromForm] BulkMerchantInsertRequest bulkMerchantInsertRequest)
    {
        if (bulkMerchantInsertRequest.Key != AccessKey)
        {
            throw new ForbiddenAccessException();
        }
        
        string[] allowedExtensions = { ".xlsx" };
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !allowedExtensions.Contains(extension.ToLowerInvariant()))
        {
            throw new InvalidFileExtensionException(_localizer.GetString("InvalidFileExtensionException"));
        }
        
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var prevalidateRequest = new BulkMerchantInsertServiceRequest(bulkMerchantInsertRequest)
        {
            ContentType = file.ContentType, FileName = file.FileName, Bytes = memoryStream.ToArray()
        };
        
        return await _bulkOperationsHttpClient.PreValidateBulkMerchantExcelFileAsync(prevalidateRequest);
    }

    /// <summary>
    /// Inserts and processes a bulk merchant excel file for import into the system,
    /// validating the file type and its metadata before completing the bulk import.
    /// </summary>
    /// <param name="file">The excel file to be imported. The file must have a valid ".xlsx" extension.</param>
    /// <param name="bulkMerchantInsertRequest">Metadata and configurations needed for the bulk import, such as access keys, merchant details, and related properties.</param>
    /// <returns>A <see cref="BulkImportMerchantResponse"/> object indicating the result of the bulk import operation.</returns>
    /// <exception cref="ForbiddenAccessException">Thrown when the provided access key does not match the expected key.</exception>
    /// <exception cref="InvalidFileExtensionException">Thrown when the file has an invalid extension or no extension at all.</exception>
    // [Authorize(Policy = "PfBulkOperations:Create")]
    [AllowAnonymous]
    [HttpPost("import-bulk-merchant")]
    [Consumes("multipart/form-data")]
    public async Task<BulkImportMerchantResponse> InsertBulkMerchantExcelFileAsync(
        [Required] IFormFile file,
        [FromForm] BulkMerchantInsertRequest bulkMerchantInsertRequest)
    {
        if (bulkMerchantInsertRequest.Key != AccessKey)
        {
            throw new ForbiddenAccessException();
        }
        
        string[] allowedExtensions = { ".xlsx" };
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !allowedExtensions.Contains(extension.ToLowerInvariant()))
        {
            throw new InvalidFileExtensionException(_localizer.GetString("InvalidFileExtensionException"));
        }
        
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var insertRequest = new BulkMerchantInsertServiceRequest(bulkMerchantInsertRequest)
        {
            ContentType = file.ContentType, FileName = file.FileName, Bytes = memoryStream.ToArray()
        };
        
        return await _bulkOperationsHttpClient.BulkImportMerchantAsync(insertRequest);
    }
}