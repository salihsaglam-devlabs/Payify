using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.Topups;

public class TopupDto : IMapFrom<CardTopupRequest>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string WalletNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardType CardType { get; set; }
    public string CardNumber { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Amount { get; set; }
    public CardTopupRequestStatus Status { get; set; }
    public DateTime CreateDate { get; set; }
    public PaymentProviderType PaymentProviderType { get; set; }
}
