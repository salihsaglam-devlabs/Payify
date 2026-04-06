using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPosRouterService
{
    Task<RouteResponse> RouteAsync(CardBinDto bin, MerchantDto merchant, string currency, int installment, decimal amount, Guid selectedVposId, bool? isInsuranceVpos = false, bool? isTopupVpos = false);
    Task<RouteResponse> OnUsRouteAsync(MerchantDto merchant, string currency, decimal amount);
    Task<Guid> CheckRouteForShortCircuitAsync(Guid merchantId, string cardNumber, string currency, int installment, decimal amount, bool? isInsuranceVpos = false);
    Task<PhysicalPosRouteResponse> PhysicalPosRouteAsync(
        Merchant merchant, 
        string bin,  
        Guid physicalPosId,
        int installment, 
        decimal amount, 
        decimal pointAmount, 
        DateTime transactionDate,
        string currency,
        TransactionType transactionType,
        MerchantTransaction referenceMerchantTransaction = null,
        decimal remainingReturnAmount = 0);
}
