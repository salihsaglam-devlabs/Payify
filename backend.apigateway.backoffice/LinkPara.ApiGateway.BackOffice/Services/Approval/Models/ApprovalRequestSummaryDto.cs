namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class ApprovalRequestSummaryDto
{
    public Guid Id { get; set; }
    public string MakerFullName { get; set; }
    public string CheckerFullName { get; set; }
    public string SecondCheckerFullName { get; set; }
}