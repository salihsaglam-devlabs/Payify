using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Application.Features.Payments.Commands.VerifyOnUsPayment;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IOnUsPaymentService
{
    Task<ValidationResponse> PreValidateOnUsAuth(ProvisionCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction, SubMerchantDto subMerchant);
    Task<ProvisionResponse> InitiateOnUsPaymentAsync(ProvisionCommand request, MerchantDto merchant, MerchantTransaction merchantTransaction, string parseUserId);
    Task TriggerOnUsWebhookAsync(Guid onUsId);
    Task<ProvisionResponse> CompleteOnUsProvisionAsync(VerifyOnUsPaymentCommand request);
    Task<PosPaymentDetailResponse> GetPaymentDetailAsync(string orderId);
    Task<PosVoidResponse> ReverseOnUsPayment(MerchantTransaction referenceMerchantTransaction);
    Task<PosRefundResponse> ReturnOnUsPayment(ReturnCommand request, MerchantTransaction referenceMerchantTransaction, Currency currency);
}