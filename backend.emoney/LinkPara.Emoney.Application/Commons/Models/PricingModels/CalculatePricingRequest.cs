using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PricingModels;

public class CalculatePricingRequest
{
    public TransferType TransferType { get; set; }
    public string CurrencyCode { get; set; }
    public int? BankCode { get; set; }
    public decimal Amount { get; set; }
    public WalletType SenderWalletType { get; set; }
}
