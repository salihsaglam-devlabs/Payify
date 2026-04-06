namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetUserLimitsQuery
{
    public Guid UserId { get; set; }
    public string CurrencyCode { get; set; }
}