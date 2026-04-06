using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;

public abstract class TopupProcessBaseRequest
{
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Guid UserId { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public string CardHolderName { get; set; }
    public string Description { get; set; }

    public abstract Task<TopupProcessResponse> ProcessAsync(
        Wallet wallet,
        CardTopupRequest cardTopupRequest,
        ITopupService topupService,
        IPaymentProviderService paymentProviderService,
        IMasterpassService masterpassService);
}
