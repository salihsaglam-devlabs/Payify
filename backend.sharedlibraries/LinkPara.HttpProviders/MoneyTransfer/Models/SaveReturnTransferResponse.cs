using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class SaveReturnTransferResponse
{
    public Guid ReferenceId { get; set; }
    public Guid TransactionSourceId { get; set; }
    public TransferType TransferType { get; set; }
}
