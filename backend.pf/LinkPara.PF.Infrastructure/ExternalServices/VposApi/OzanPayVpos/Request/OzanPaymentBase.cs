namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;

public class OzanPaymentBase : OzanPayRequestBase
{
    public string ProviderKey { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; }
    public string Number { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string Cvv { get; set; }
    public string Description { get; set; }
    public bool Is3D { get; set; }
    public bool Only3D { get; set; }
    public bool IsPreAuth { get; set; }
    public int Installment { get; set; }
    public string BillingFirstName { get; set; }
    public string BillingLastName { get; set; }
    public string Email { get; set; }
    public string BillingCompany { get; set; }
    public string BillingPhone { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingCountry { get; set; }
    public string BillingCity { get; set; }
    public string BillingPostCode { get; set; }
    public List<object> BasketItems { get; set; }
    public string ReturnUrl { get; set; }
    public string CustomerIp { get; set; }
    public string CustomerUserAgent { get; set; }
    public object BrowserInfo { get; set; }
}
