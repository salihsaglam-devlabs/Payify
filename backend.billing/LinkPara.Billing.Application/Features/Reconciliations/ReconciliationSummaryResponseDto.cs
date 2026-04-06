using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;

namespace LinkPara.Billing.Application.Features.Reconciliations;

public class ReconciliationSummaryResponseDto : IMapFrom<BillingResponse<ReconciliationSummaryResponse>>
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ReconciliationSummaryResponse Response { get; set; }
}