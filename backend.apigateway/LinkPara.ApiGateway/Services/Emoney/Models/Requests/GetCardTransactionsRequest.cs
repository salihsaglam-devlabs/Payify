namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetCardTransactionsRequest 
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public string ConsentId { get; set; }
    public string CardRefNo { get; set; }
    public int PeriodValue { get; set; }
    public string StatementType { get; set; }
    public int PageRecordCount { get; set; }
    public int PageNo { get; set; }
    public string DebtOrCredit { get; set; }
    public string OrderType { get; set; }
}