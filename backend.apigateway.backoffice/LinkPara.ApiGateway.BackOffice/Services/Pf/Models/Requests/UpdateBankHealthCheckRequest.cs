namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateBankHealthCheckRequest
{
    public Guid Id { get; set; }
    public bool IsHealthCheckAllowed { get; set; }
}
