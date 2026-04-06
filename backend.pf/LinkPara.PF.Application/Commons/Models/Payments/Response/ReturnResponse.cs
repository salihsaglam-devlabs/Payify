using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class ReturnResponse : ResponseBase
{
    public string ReturnMessage { get; set; }
    public ReturnApprovalStatus ApprovalStatus { get; set; }
    public string ProvisionNumber { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public decimal ReturnAmount { get; set; }
}