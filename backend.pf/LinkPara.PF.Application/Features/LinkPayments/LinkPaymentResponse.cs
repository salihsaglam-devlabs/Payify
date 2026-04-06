
using LinkPara.PF.Application.Commons.Models.Payments.Response;

namespace LinkPara.PF.Application.Features.LinkPayments;

public class LinkPaymentResponse : ResponseBase
{
    public string LinkUrlPath { get; set; }
    public string OrderId { get; set; }
}
