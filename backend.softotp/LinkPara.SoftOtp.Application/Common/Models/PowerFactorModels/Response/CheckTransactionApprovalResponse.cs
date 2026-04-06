using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Enums;

namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;

public class CheckTransactionApprovalResponse : PowerFactorResponseBase
{
    public TransactionApprovalStatus Status { get; set; }
}