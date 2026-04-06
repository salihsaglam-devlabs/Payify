using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class PostingBalanceStatisticsRequest
{
    public Guid? MerchantId { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public DateTime? PaymentDateStart { get; set; }
    public DateTime? PaymentDateEnd { get; set; }
    public PostingMoneyTransferStatus?[] MoneyTransferStatus { get; set; }
}