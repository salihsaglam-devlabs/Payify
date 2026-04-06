namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetAccountLimitsQuery
{
    public Guid AccountId { get; set; }
    public string CurrencyCode { get; set; }
}