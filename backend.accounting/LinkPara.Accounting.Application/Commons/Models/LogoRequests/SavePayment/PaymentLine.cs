namespace LinkPara.Accounting.Application.Commons.Models.LogoRequests.SavePayment;

public class PaymentLine
{
    public string CUSTOMER_ID { get; set; }
    public string TRCODE { get; set; }
    public string DIRECTION { get; set; }
    public string FICHENO { get; set; }
    public string ACCOUNTNO { get; set; }
    public decimal AMOUNT { get; set; }
    public string CURRENCYCODE { get; set; }
    public decimal TRY_RATE { get; set; }
    public string LINEEXP { get; set; }
    public string GENEXP1 { get; set; }
    public string GENEXP2 { get; set; }
    public string GENEXP3 { get; set; }
    public string GENEXP4 { get; set; }
    public string SRV_CODE { get; set; }
}
