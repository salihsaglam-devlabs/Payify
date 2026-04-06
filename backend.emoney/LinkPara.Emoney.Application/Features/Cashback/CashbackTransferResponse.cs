using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Cashback;

public class CashbackTransferResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
    public decimal PaymentAmount { get; set; }
    public Account ReceiverAccount { get; set; }

}
