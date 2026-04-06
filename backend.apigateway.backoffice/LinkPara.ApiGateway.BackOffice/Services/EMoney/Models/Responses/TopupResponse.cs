using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class TopupResponse
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
