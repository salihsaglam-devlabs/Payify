using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class GetTransferBankResponse
{
    public int IbanBankCode { get; set; }
    public string IbanBankName { get; set; }
    public int TransferBankCode { get; set; }
    public string TransferBankName { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime AcceptableTransferDate { get; set; }
    public bool IsEftSuitableNow { get; set; }
}
