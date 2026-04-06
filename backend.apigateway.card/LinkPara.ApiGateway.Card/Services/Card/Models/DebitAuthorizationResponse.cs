namespace LinkPara.ApiGateway.Card.Services.Card.Models;
public class DebitAuthorizationResponse
{
    public long CorrelationID { get; set; }
    public string BankingRefNo { get; set; }
    public List<BalanceInfo> BalanceInformationList { get; set; }
    public TransactionAmount TransactionAmount { get; set; }
    public TransactionAmount BillingAmount { get; set; }
    public List<Fee> FeeList { get; set; }
    public string ResponseCode { get; set; } = default!;
    public string ResponseDescription { get; set; }
    public string ResponseMessage { get; set; }
    public bool IsApproved { get; set; }

}
