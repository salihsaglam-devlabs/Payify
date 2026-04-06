namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Response;

public class TransactionApiResponse : BaseApiResponse
{
    public object ExtraInfo { get; set; }
    public TransactionResult Result { get; set; }
}
public class TransactionResult
{
    public string TransactionId { get; set; }
    public string ResultMessage { get; set; }
    public string TriggeredAlarm { get; set; }
    public int TotalScore { get; set; }
    public Triggeredruleslist[] TriggeredRulesList { get; set; }
}

public class Triggeredruleslist
{
    public string RuleKey { get; set; }
    public int Score { get; set; }
}
