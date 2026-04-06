namespace LinkPara.Accounting.Application.Commons.Models.LogoRequests.SavePayment;

public class SavePaymentRequest
{
    public DateTime DATE_ { get; set; }
    public string REFERANCE { get; set; }
    public int OPR_TYPE { get; set; }
    public List<PaymentLine> lines { get; set; }
}
