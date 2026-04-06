using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Interfaces;

public interface ISekerBankApi
{
    Task<SekerBankResponse<SekerBankAuthorizationResponse>> LoginAsync(SekerBankAuthorizationRequest request);
    Task<SekerBankResponse<List<SekerBankInstitutionListResponse>>> GetInstitutionListAsync(string authorizationToken);
    Task<SekerBankResponse<SekerBankBillInquiryResponse>> InquireBillsAsync(string authorizationToken, SekerBankBillInquiryRequest request);
    Task<SekerBankResponse<SekerBankBillPaymentResponse>> PayInquiredBillsAsync(string authorizationToken, SekerBankBillPaymentRequest request);
    Task<SekerBankResponse<SekerBankBillStatusResponse>> InquireBillStatusAsync(string authorizationToken, SekerBankBillStatusRequest request);
    Task<SekerBankResponse<SekerBankBillPaymentCancelResponse>> CancelBillPaymentAsync(string authorizationToken, SekerBankBillPaymentCancelRequest request);
    Task<SekerBankResponse<SekerBankReconciliationSummaryResponse>> GetReconciliationSummaryAsync(string authorizationToken, SekerBankReconciliationSummaryRequest request);
    Task<SekerBankResponse<SekerBankReconciliationDetailsResponse>> GetReconciliationDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request);
    Task<SekerBankResponse<SekerBankInstitutionReconciliationDetailsResponse>> GetInstitutionReconciliationDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request);
    Task<SekerBankResponse<SekerBankInstitutionPaymentDetailsResponse>> GetInstitutionPaymentDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request);
    Task<SekerBankResponse<SekerBankInquireInstitutionReconciliationResponse>> InquireInstitutionReconciliationStatusAsync(string authorizationToken, SekerBankInquireInstitutionReconciliationRequest request);
}