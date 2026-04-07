namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
public class CustomerLimit
{
    public int CurrencyCode { get; set; }
    public float CurrentLimit { get; set; }
    public float LimitRatio { get; set; }
    public string UsageType { get; set; }
    public DateTime LcsSectorFirstUsedDate { get; set; }
    public bool IsOverlimitAllowed { get; set; }
    public string LimitAssignType { get; set; }
    public int ResetPeriod { get; set; }
}
