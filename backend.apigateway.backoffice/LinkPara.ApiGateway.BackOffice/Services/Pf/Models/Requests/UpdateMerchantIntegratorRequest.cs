namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMerchantIntegratorRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal CommissionRate { get; set; }
}
