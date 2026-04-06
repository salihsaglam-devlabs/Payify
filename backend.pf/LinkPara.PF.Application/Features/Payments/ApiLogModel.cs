using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;

namespace LinkPara.PF.Application.Features.Payments;

public class ApiLogModel : IMapFrom<MerchantApiLog>
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public PaymentLogType PaymentType { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CreateDate { get; set; }
}
