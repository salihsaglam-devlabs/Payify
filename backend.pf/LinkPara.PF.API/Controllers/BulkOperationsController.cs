using LinkPara.PF.Application.Features.BulkOperations.Merchants;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkImportMerchant;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkMerchantExcelValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class BulkOperationsController : ApiControllerBase
{
    /// <summary>
    /// Pre-validates the provided bulk merchant Excel file to ensure it meets the required format and data criteria.
    /// </summary>
    /// <param name="command">Contains bytes of the Excel file to be validated and merchant metadata.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a validation result indicating success or failure, and any error messages or warnings found.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("prevalidate-bulk-merchant")]
    public async Task<ActionResult<BulkMerchantExcelValidationResponse>> PreValidateBulkMerchantExcelFileAsync(BulkMerchantExcelValidationCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Asynchronously processes the bulk import of merchant data.
    /// This method handles the ingestion of multiple merchant records
    /// and ensures they are properly processed and saved to the datastore.
    /// </summary>
    /// <param name="command">The file path containing the merchant data to be imported.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a status indicating the success of the import operation.</returns>
    [AllowAnonymous]
    [HttpPost("import-bulk-merchant")]
    public async Task<ActionResult<BulkImportMerchantResponse>> BulkImportMerchantAsync(BulkImportMerchantCommand command)
    {
        return await Mediator.Send(command);
    }
}