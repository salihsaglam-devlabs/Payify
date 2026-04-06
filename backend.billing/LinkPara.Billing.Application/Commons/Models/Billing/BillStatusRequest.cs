using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillStatus;

namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillStatusRequest : BillingTransaction , IMapFrom<GetBillStatusQuery>
{
    public string BillId { get; set; }
}