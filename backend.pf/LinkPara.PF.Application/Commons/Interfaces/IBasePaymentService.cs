using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IBasePaymentService
{
    Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode);
    PricingProfile GetPricingProfileByTransaction(MerchantDto merchant, Currency currency);
    PricingProfileItem GetPricingProfileItemByTransaction(PricingProfile pricingProfile, MerchantTransaction merchantTransaction);
    Task<ValidationResponse> CheckLimitControlAsync(ProvisionCommand request, VposPaymentType paymentType);
    Task<ProvisionResponse> GetProvisionResponseAsync(string code, ProvisionCommand request);
    Task<string> GenerateOrderNumberAsync(Guid merchantId, string orderNumber);
    Task<DateTime> CalculatePaymentDateAsync(DateTime transactionDate, int dueDate);
    Task InsertTimeoutTransactionAsync(MerchantTransaction merchantTransaction, BankTransaction bankTransaction, string clientIpAddress, string originalOrderId = null);
    Task PublishIncrementLimitAsync(Guid merchantTransactionId);
    Task<bool> GetIsPaymentDateWillBeShiftedAsync(DateTime transactionEndDate, int acquireBankCode);
}