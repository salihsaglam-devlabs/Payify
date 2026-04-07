namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreGetCardInformationResponse
{
    public string ApplicationRefNo { get; set; }
    public string BankingCustomerNo { get; set; }
    public string BarcodeNo { get; set; }
    public string BatchBarcodeNo { get; set; }
    public string CardNo { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerType { get; set; }
    public string Dci { get; set; }
    public string ProductCode { get; set; }
    public string Segment { get; set; }
}
