using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Billing;

namespace LinkPara.Billing.Application.Features.Billing;

public class BillCancelResponseDto : IMapFrom<BillingResponse<BillCancelResponse>>
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public BillCancelResponse Response { get; set; }
}