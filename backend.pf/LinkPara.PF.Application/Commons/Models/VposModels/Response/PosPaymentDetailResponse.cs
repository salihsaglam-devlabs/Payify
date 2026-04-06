using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.VposModels.Response;

public class PosPaymentDetailResponse : PosResponseBase
{
    public string BatchNo { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string Installment { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CardInformation { get; set; }
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
}