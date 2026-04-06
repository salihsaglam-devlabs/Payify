namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Responses;

public class PaycoreGetCustomerCardsResponse
{
    public CustomerCards[] CustomerCards { get; set; }
}
public class CustomerCards
{
    public string BankingCustomerNo { get; set; }
    public string CustomerNo { get; set; }
    public string CardNo { get; set; }
    public string PrevCardNo { get; set; }
    public string MainCardNo { get; set; }
    public string PhysicalType { get; set; }
    public string CardLevel { get; set; }
    public int BranchCode { get; set; }
    public string Dci { get; set; }
    public string Segment { get; set; }
    public string ProductName { get; set; }
    public string ProductCode { get; set; }
    public string LogoCode { get; set; }
    public string StatCode { get; set; }
    public string StatDescription { get; set; }
    public string StatusReason { get; set; }
    public string EmbossCode { get; set; }
    public DateTime EmbossDate { get; set; }
    public string EmbossName1 { get; set; }
    public string EmbossName2 { get; set; }
    public string ApplicationRefNo { get; set; }
    public string BarcodeNo { get; set; }
    public string BankAccountNo { get; set; }
}

