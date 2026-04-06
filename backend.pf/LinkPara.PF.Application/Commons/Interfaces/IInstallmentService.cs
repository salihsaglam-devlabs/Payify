using LinkPara.PF.Application.Features.Installments;
using LinkPara.PF.Application.Features.Installments.Queries.CalculateInstallmentPricing;
using LinkPara.PF.Application.Features.Installments.Queries.GetManualPaymentPageInstallments;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IInstallmentService
{
    public Task<InstallmentPricingResponse> GetInstallmentPricingAsync(CalculateInstallmentPricingQuery request);
    public Task<ManualPaymentPageInstallmentsResponse> GetManualPaymentPageInstallmentsAsync(GetManualPaymentPageInstallmentsQuery request);
}