using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;

public class TransferRequestCreated
{
    public TransactionSource Source { get; set; }
    public Guid TransactionSourceReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string CurrencyCode { get; set; }
}
