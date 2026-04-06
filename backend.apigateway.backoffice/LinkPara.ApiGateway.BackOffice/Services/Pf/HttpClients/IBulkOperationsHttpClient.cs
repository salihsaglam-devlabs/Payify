using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IBulkOperationsHttpClient
{
    Task<BulkMerchantExcelValidationResponse> PreValidateBulkMerchantExcelFileAsync(BulkMerchantInsertServiceRequest request);
    Task<BulkImportMerchantResponse> BulkImportMerchantAsync(BulkMerchantInsertServiceRequest request);
}