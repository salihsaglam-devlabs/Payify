using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class EmoneySavePricingProfileRequest
{
    public DateTime ActivationDateStart { get; set; }
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType TransferType { get; set; }
    public List<PricingProfileItemModel> ProfileItems { get; set; }
    public CardType CardType { get; set; }
}

public class PricingProfileItemModel
{
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}
