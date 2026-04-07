namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

public class GetCustomerLimitInfoResponse
{
    public int limit { get; set; }
    public int availableLimit { get; set; }
    public int customerLimit { get; set; }
    public int customerAvailableLimit { get; set; }
    public int currency { get; set; }
    public bool isInOverLimit { get; set; }
    public Limitext[] limitExts { get; set; }
}
public class Limitext
{
    public string limitType { get; set; }
    public int limit { get; set; }
    public int limitRate { get; set; }
    public int availableLimit { get; set; }
}