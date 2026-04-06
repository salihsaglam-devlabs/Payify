namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;
public class UpdateCustomerLimitRequest
{
    public string BankingCustomerNo { get; set; }
    public int NewLimit { get; set; }
    public string MemoText { get; set; }
    public string LimitAssignType { get; set; }
    public int CurrencyCode { get; set; }
    public bool IsLimitUsedControl { get; set; }
}
