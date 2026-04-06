namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class AccTranDto
{
    public string AccountRef { get; set; }
    public string InstanceId { get; set; }
    public string TranRefNo { get; set; }
    public string TranAmount { get; set; }
    public string Currency { get; set; }
    public string TranTime { get; set; }
    public string Channel { get; set; }
    public string DebitOrCredit { get; set; } 
    public string TransactionType { get; set; } 
    public string TransactionPurpose { get; set; } 
    public string PaymentStmNo { get; set; }
    public string Explanation { get; set; }
    public string OtherMaskedIban { get; set; }
    public string OtherTitle { get; set; }

}

