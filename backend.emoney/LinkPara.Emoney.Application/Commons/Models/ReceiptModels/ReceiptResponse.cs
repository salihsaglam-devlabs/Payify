namespace LinkPara.Emoney.Application.Commons.Models.ReceiptModels;

public class ReceiptResponse
{
    public bool IsReady { get; set; }

    public string CustomerNumber { get; set; }
    public string ReceiptNumber { get; set; }

    public ReceiptParty Sender { get; set; }
    public ReceiptParty Receiver { get; set; }

    public ReceiptTransaction Transaction { get; set; }
    public ReceiptAmounts Amounts { get; set; }
    public ReceiptCompanyInfo CompanyInfo { get; set; }
}
