namespace LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;

public class TransactionApiResponse : BaseResponse
{
    public object ExtraInfo { get; set; }
    public TransactionResult Result { get; set; }
}
public class TransactionResult
{
    public string TransactionId { get; set; }
    public string ResultMessage { get; set; }
    public string TriggeredAlarm { get; set; }
    public decimal TotalScore { get; set; }
    public TriggeredRulesList[] TriggeredRulesList { get; set; }
}

public class TriggeredRulesList
{
    public string RuleKey { get; set; }
    public decimal Score { get; set; }
}