namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetFuturePaymentOrderListRequest
{    
    public string ApplicationUser { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
}
