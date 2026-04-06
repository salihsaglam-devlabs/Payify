namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class OnUsPaymentApproveRequest
{
    public Guid OnUsPaymentRequestId { get; set; }
    public string SenderWalletNumber { get; set; }
    public bool IsVerifiedByUser { get; set; }
}