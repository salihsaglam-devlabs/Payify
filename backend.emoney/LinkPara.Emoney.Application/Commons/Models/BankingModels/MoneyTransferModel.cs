using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.BankingModels;

public class MoneyTransferModel
{
    public int TransferBankCode { get; set; }
    public string TransferBankName { get; set; }
    public int IbanBankCode { get; set; }
    public string IbanBankName { get; set; }
    public TransferType TransferType { get; set; }
}
