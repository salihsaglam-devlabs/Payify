using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IBillingService
{
    Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery request);
    Task<BillingResponse<BillPaymentResponse>> PayInquiredBillAsync(PayInquiredBillCommand request);
    Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(Guid transactionId, string cancellationReason);
    Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(Guid transactionId);
    Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(ReconciliationSummaryRequest request);
    Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest request);
    Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(Guid institutionSummaryId);
    Task<BillingResponse<InstitutionReconciliationCloseResponse>> CloseInstitutionReconciliationAsync(InstitutionReconciliationCloseRequest request);
    Task<BillingResponse<bool>> DoReconciliationAsync(ReconciliationJobCommand request);
}