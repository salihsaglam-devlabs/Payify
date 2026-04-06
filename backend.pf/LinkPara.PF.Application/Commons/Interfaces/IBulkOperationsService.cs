using LinkPara.PF.Application.Features.BulkOperations.Merchants;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkImportMerchant;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkMerchantExcelValidation;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IBulkOperationsService
{
    Task<BulkMerchantExcelValidationResponse> PreValidateBulkMerchantExcelFileAsync(BulkMerchantExcelValidationCommand command);
    Task<BulkImportMerchantResponse> BulkImportMerchantAsync(BulkImportMerchantCommand command);
}