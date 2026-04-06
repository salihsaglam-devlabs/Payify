namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;

public class CustomerConfirmationRequest
{
    public Contract Contract { get; set; }
}

public class Contract
{
    public string ConsentType { get; set; }
    public string CustomerType { get; set; }
    public string IdentityType { get; set; }
    public string IdentityValue { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }
    public decimal Amount { get; set; }
    public string SenderIban { get; set; }
    public string ReceiverIban { get; set; }
    public string SenderAccRef { get; set; }
    public string SenderTitle { get; set; }
    public string ReceiverTitle { get; set; }
    public string AddressType { get; set; }
    public string AddressValue { get; set; }
    public string PaymentResource { get; set; }
    public string PaymentPurpose { get; set; }
    public string Currency { get; set; }
    public string DecoupledIdType { get; set; }
    public string DecoupledIdValue { get; set; }
}
