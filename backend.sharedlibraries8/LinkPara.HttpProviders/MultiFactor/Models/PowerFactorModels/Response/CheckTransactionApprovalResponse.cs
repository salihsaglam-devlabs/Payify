using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;

namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

public class CheckTransactionApprovalResponse : PowerFactorResponseBase
{
    public TransactionApprovalStatus Status { get; set; }
}