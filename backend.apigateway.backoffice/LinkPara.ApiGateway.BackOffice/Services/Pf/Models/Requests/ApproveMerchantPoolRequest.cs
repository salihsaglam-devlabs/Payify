using LinkPara.ApiGateway.BackOffice.Commons.Helpers;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class ApproveMerchantPoolRequest
{
    public Guid MerchantPoolId { get; set; }
    public bool IsApprove { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
}

public class ApproveMerchantPoolServiceRequest : ApproveMerchantPoolRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
