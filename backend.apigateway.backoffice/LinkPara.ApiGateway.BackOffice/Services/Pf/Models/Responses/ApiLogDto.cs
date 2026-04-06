using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class ApiLogDto
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
