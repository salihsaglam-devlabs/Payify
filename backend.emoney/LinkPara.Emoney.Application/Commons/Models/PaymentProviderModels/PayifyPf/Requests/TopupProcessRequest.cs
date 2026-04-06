using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class TopupProcessRequest : TopupProcessBaseRequest
{
    public string CardToken { get; set; }

    public override async Task<TopupProcessResponse> ProcessAsync(
        Wallet wallet,
        CardTopupRequest cardTopupRequest,
        ITopupService topupService,
        IPaymentProviderService paymentProviderService,
        IMasterpassService masterpassService)
    {
        return await paymentProviderService.TopupProcessAsync(this, wallet, cardTopupRequest, topupService);
    }
}