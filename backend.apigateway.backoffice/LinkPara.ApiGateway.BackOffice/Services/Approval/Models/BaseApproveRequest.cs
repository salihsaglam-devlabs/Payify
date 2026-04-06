
namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class BaseApproveRequest
{
    public Guid UserId { get; set; }
    public Guid RequestId { get; set; }
    public string Description { get; set; }

}
