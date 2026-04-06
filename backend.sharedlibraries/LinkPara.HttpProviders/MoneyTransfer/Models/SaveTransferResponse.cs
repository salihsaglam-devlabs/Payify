using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class SaveTransferResponse
{
    public Guid ReferenceId { get; set; }
    public Guid TransactionSourceId { get; set; }
    public int IbanBankCode { get; set; }
    public string IbanBankName { get; set; }
    public int TransferBankCode { get; set; }
    public string TransferBankName { get; set; }
    public TransferType TransferType { get; set; }
    public bool IsReturnPayment { get; set; }
    public DateTime WorkingDate { get; set; }
}
