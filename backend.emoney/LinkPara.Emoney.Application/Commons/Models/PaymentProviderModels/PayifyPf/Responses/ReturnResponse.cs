using LinkPara.Emoney.Application.Commons.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class ReturnResponse : ResponseModel
{
    public string ReturnMessage { get; set; }
    public ReturnApprovalStatus ReturnApprovalStatus { get; set; }
    public string ProvisionNumber { get; set; }
}
