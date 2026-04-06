using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;

public abstract class TopupCancelBaseRequest
{
    public Guid CardTopupRequestId { get; set; }
    public string Description { get; set; }

    public abstract Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService);
}
