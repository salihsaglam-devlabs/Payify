namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class UpdateTransferBankRequest
{
    public Guid MoneyTransferTransactionId { get; set; }
    public int TransferBankCode { get; set; }
}
