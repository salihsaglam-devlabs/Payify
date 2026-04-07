namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Response;

public class GetCustomerLimitInfoResponse
{
    public int CurrencyCode { get; set; }
    public decimal CurrentLimit { get; set; }
    public decimal LimitRatio { get; set; }
    public string UsageType { get; set; }
}