using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.LinkPayments;

public class LinkPaymentDetailResponse
{
    public string OrderId { get; set; }
    public TransactionType TransactionType { get; set; }
    public ChannelPaymentStatus LinkPaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentCount { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerNote { get; set; }
}
