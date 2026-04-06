using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Infrastructure.CorporationPaymentsService;

namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Interfaces;

public interface IVakifKatilimApi
{
    Task<LinkPara.Billing.Infrastructure.CorporationPaymentsService.InstitutionList[]> GetInstitutionListAsync();
    Task<ResponseBillInquiry> InquireBillsAsync(int institutionId, string firstQueryField, Vendor vendor);
    
    Task<ResponseBillCancel> CancelPaymentAsync(int paymentId);

    Task<ResponseBillControl> ControlPaymentAsync(int paymentId);

    Task<ResponseBillPayment> DoPaymentAsync(int paymentId, BillList bill, PayInquiredBillCommand payInquiredBillCommand, string identityNumber);

    Task<ResponseReconciliationSummary> ReconciliationSummaryAsync(DateTime dateTime);
    
    Task<ResponseReconciliationSummary> ReconciliationSummaryByInstitutionAsync(DateTime dateTime, int? institutionId);

    Task<ResponseReconciliationDetail> ReconcilationSummaryDetailAsync(DateTime dateTime,
        int institutionId, bool getOnlyFaultyTransactions);

}