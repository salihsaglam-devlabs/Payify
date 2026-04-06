using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;

namespace LinkPara.Billing.Application.Features.Reconciliations;

public class InstitutionPaymentDetailResponseDto : IMapFrom<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>>
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ReconciliationInstitutionPaymentDetailsSummaryResponse Response { get; set; }
}