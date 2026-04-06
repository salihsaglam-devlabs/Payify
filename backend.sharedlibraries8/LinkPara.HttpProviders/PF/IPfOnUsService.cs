using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.HttpProviders.PF;

public interface IPfOnUsService
{
    Task<VerifyOnUsPaymentResponse> VerifyOnUsPaymentAsync(VerifyOnUsPaymentRequest request);
    Task<UpdateMerchantTransactionRequest> ChargebackOnUsPayment(Guid merchantTransactionId, JsonPatchDocument<UpdateMerchantTransactionRequest> merchantTransaction);
    Task<PaginatedList<MerchantDeductionDto>> GetOnUsPaymentDeductions(GetOnUsPaymentDeductionsRequest merchantDeductionsRequest);
}