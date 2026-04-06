using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;

public class IncomingTransactionProcessCompleted
{
    public bool IsSucceeded { get; set; }
    public Guid IncomingTransactionId { get; set; }
    public IncomingTransactionStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverWalletNumber { get; set; }
}
