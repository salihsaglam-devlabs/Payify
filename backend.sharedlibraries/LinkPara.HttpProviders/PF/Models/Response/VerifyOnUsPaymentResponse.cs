namespace LinkPara.HttpProviders.PF.Models.Response;

public class VerifyOnUsPaymentResponse
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public string ProvisionNumber { get; set; }
    public string Description { get; set; }
}