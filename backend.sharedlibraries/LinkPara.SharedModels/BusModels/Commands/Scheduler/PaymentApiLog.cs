using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;

namespace LinkPara.SharedModels.BusModels.Commands.Scheduler;

public class PaymentApiLog
{
    public Guid MerchantId { get; set; }
    public PaymentLogType PaymentType { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
