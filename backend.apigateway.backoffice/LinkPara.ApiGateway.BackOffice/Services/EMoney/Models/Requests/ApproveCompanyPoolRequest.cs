using LinkPara.ApiGateway.BackOffice.Commons.Helpers;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class ApproveCompanyPoolRequest
{
    public Guid CompanyPoolId { get; set; }
    public bool IsApprove { get; set; }
    public string RejectReason { get; set; }
}
public class ApproveCompanyPoolServiceRequest : ApproveCompanyPoolRequest, IHasUserId
{
    public Guid UserId { get; set; }
}

