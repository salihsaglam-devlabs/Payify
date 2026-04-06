namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetAccountLimitsRequest
{
    public Guid AccountId { get; set; }
    public string CurrencyCode { get; set; }
}