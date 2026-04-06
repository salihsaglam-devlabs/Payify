namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class BalanceLimitDto
{
    public bool MaxBalanceLimitEnabled { get; set; }
    public decimal MaxBalance { get; set; }
    public decimal CurrentBalance { get; set; }
}