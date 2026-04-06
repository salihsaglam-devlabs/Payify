using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.Commands.Emoney;

public class ProcessWalletReturn
{
    public Guid MoneyTransferTransactionId { get; set; }
    public Guid TransactionId { get; set; }
    public ReturnReason ReturnReason;
}
