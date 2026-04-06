using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IBillingVendorService
{
    public Task<List<SectorMapping>> GetSectorListAsync();
    public Task<List<InstitutionMapping>> GetInstitutionListAsync();
    public Task<List<Institution>> GetInstitutionListBySectorAsync(Guid sectorId);
    public Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery billInquiryRequest); 
    public Task<BillingResponse<BillPaymentResponse>> PayInquiredBillsAsync(PayInquiredBillCommand billPaymentRequest);
    public Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(BillCancelRequest billCancelRequest);
    public Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(BillStatusRequest billStatusRequest); 
    public Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(TransactionStatistics statistics); 
    public Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest reconcilationDetailsRequest); 
    public Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(ReconcilliationInstitutionDetailRequest reconciliationDetailsRequest);
    public Task<BillingResponse<InstitutionReconciliationCloseResponse>> InstitutionReconciliationCloseAsync(InstitutionReconciliationCloseRequest institutionReconciliationCloseRequest);
    public Task ValidateRequestAsync(Guid institutionId, string subscriberNumber1, string subscriberNumber2, string subscriberNumber3);
}