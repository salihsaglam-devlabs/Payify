using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class EmoneyPricingProfileDto
{
    public Guid Id { get; set; }
    public PricingProfileStatus Status { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string Name { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime ActivationDateStart { get; set; }
    public DateTime? ActivationDateEnd { get; set; }
    public int? BankCode { get; set; }
    public Guid? BankId { get; set; }
    public BankDto Bank { get; set; }
    public Guid CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; }
    public CardType CardType { get; set; }

    public List<EmoneyPricingProfileItemDto> PricingProfileItems { get; set; }
}

public class EmoneyPricingProfileItemDto
{
    public Guid Id { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}
