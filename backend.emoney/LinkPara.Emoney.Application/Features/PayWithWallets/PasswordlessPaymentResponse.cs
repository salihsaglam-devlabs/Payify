namespace LinkPara.Emoney.Application.Features.PayWithWallets;

public class PayWithWalletResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
    public decimal PaymentAmount { get; set; }

}
