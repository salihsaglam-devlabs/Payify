using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class TopupCancelRequest : TopupCancelBaseRequest
{
    public override async Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService)
    {
        return await paymentProvider.TopupCancelAsync(cardTopupRequest, wallet, description, amount, paymentProvider, masterpassService, topupService);
    }
}
