namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class TransferForLoggedInUserRequest
{
    public string PaymentReferenceId { get; set; }
    public string SenderPhoneNumber { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string PartnerNumber { get; set; }
}
