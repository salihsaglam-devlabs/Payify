namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class GetCardLastCourierActivityRequest
{
    public string CardNumber { get; set; }
    public string BankingCustomerNumber { get; set; }
    public string BarcodeNumber { get; set; }
    public string BatchBarcodeNumber { get; set; }
    public string CustomerNumber { get; set; }
    public string DCI { get; set; }
    public string ProductCode { get; set; }
}