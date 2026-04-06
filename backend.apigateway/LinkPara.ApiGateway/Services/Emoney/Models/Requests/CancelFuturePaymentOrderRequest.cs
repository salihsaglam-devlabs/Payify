namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class CancelFuturePaymentOrderRequest
{
    public string ConsentId { get; set; }    
    public string HhsCode { get; set; }
    public string ApplicationUser { get; set; }    
}
