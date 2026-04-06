using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPricingCommercialService
{
    public Task<CalculatePricingResponse> CalculateCustomPricingAsync(decimal amount, decimal fee, decimal commissionRate);
    public Task<bool> CheckIfAccountIsCommercialNowAsync(string currencyCode, PricingCommercialType pricingCommercialType, Guid receiverAccountId, bool ownAccount);
    public Task SendCommercialInfoPushNotificationAsync(Account receiverAccount, PricingCommercial pricingCommercial);
    public Task<PricingCommercial> GetPricingCommercialRateAsync(string currencyCode, PricingCommercialType pricingCommercialType);
    public Task<bool> IsGreaterThanMinAmountLimit(decimal amount);
}