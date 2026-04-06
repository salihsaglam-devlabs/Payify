using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.Banks;
using LinkPara.Emoney.Application.Features.Currencies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.PricingProfiles;

public class PricingProfileDto : IMapFrom<PricingProfile>
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
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}
