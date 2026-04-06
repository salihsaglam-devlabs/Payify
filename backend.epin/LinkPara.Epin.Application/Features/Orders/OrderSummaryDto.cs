using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Application.Features.Orders;

public class OrderSummaryDto : IMapFrom<Order>
{
    public string PhoneNumber { get; set; }
    public string Pin { get; set; }
}
