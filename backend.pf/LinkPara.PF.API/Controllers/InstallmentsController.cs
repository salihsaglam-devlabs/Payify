using LinkPara.PF.Application.Features.Installments;
using LinkPara.PF.Application.Features.Installments.Queries.CalculateInstallmentPricing;
using LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;
using LinkPara.PF.Application.Features.Links;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.PF.Application.Features.Installments.Queries.GetManualPaymentPageInstallments;

namespace LinkPara.PF.API.Controllers;

public class InstallmentsController : ApiControllerBase
{
    /// <summary>
    /// Get Installments info of a merchant with pricings.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Installment:ReadAll")]
    [HttpGet("")]
    public async Task<InstallmentPricingResponse> CalcuateInstallmentPricings([FromQuery] CalculateInstallmentPricingQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Get Manual Payment Page Installments.
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Installment:ReadAll")]
    [HttpGet("ManualPaymentPage/{merchantId}")]
    public async Task<ManualPaymentPageInstallmentsResponse> GetManualPaymentPageInstallments(Guid merchantId)
    {
        return await Mediator.Send(new GetManualPaymentPageInstallmentsQuery { MerchantId = merchantId });
    }
}