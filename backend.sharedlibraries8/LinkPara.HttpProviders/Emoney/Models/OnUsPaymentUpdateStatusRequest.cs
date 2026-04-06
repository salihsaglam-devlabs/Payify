using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;

public class OnUsPaymentUpdateStatusRequest
{
    public Guid OnUsPaymentRequestId { get; set; }
    public OnUsPaymentStatus Status { get; set; }    
}
