namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class DeactivateAccountRequest
{
    public Guid AccountId { get; set; }
    public string ChangeReason { get; set; }
}