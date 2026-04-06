namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class UpdateTierPermissionRequest
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
}