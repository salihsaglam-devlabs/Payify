using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class TopupPricingProfileRequest
{
    public DateTime ActivationDateStart { get; set; }
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TopupPricingProfileItemModel ProfileItem { get; set; }
    public CardType CardType { get; set; }
}

public class TopupPricingProfileItemModel
{
    public decimal MinAmount { get; set; } = 1;
    public decimal MaxAmount { get; set; } = 999999;
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}
