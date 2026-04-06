using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IInstallmentHttpClient
{
    public Task<InstallmentPricingResponse> CalcuateInstallmentPricings(InstallmentPricingRequest query);
    public Task<InstallmentsManualPaymentPageResponse> GetManualPaymentPageInstallments(Guid merchantId);
}