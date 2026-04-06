using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Application.Commons.Models.Billing;

namespace LinkPara.Billing.Application.Features.Billing;

public class BillCancelRequestDto : IMapFrom<BillCancelRequest>
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Bill Bill { get; set; }
}