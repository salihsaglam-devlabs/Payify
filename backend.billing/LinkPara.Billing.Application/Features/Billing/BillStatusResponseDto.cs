using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Billing;

namespace LinkPara.Billing.Application.Features.Billing;

public class BillStatusResponseDto : IMapFrom<BillingResponse<BillStatusResponse>>
{
    public bool IsSuccess { get; set; }
    public string Description { get; set; }
    public BillStatusResponse Response { get; set; }
}
