using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;

public class WalletReturnCompleted
{
    public Guid MoneyTransferReferenceId { get; set; }
    public ReturnReason ReturnReason { get; set; }
}
