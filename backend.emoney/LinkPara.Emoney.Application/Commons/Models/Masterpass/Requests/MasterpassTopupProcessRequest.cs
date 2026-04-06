using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class MasterpassTopupProcessRequest : TopupProcessBaseRequest
{
    public string OrderId { get; set; }
    public string AccountKey { get; set; }
    public int InstallmentCount { get; set; } = 0;
    public string RequestReferenceNo { get; set; }
    public string AcquirerIcaNumber { get; set; }
    public string Token { get; set; }
    public MasterpassTransactionType TransactionType { get; set; }
    public string CardNumber { get; set; }

    public override async Task<TopupProcessResponse> ProcessAsync(Wallet wallet, CardTopupRequest cardTopupRequest, ITopupService topupService, IPaymentProviderService paymentProviderService, IMasterpassService masterpassService)
    {
        return await masterpassService.TopupProcessAsync(this, wallet, cardTopupRequest, topupService);
    }
}