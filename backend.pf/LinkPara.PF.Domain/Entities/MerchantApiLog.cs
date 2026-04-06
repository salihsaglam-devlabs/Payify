using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantApiLog : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public PaymentLogType PaymentType { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
