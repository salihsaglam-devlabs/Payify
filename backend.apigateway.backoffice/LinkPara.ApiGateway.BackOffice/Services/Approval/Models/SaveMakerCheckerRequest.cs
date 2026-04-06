namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class SaveMakerCheckerRequest
{
    public Guid CaseId { get; set; }
    public Guid MakerRoleId { get; set; }
    public Guid CheckerRoleId { get; set; }
    public Guid SecondCheckerRoleId { get; set; }
}
