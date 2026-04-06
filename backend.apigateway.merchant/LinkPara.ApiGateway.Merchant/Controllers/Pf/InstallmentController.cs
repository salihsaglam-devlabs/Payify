using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class InstallmentController: ApiControllerBase
{
    private readonly IInstallmentHttpClient _installmentHttpClient;

    public InstallmentController(IInstallmentHttpClient installmentHttpClient)
    {
        _installmentHttpClient = installmentHttpClient;
    }
    
    /// <summary>
    /// Returns installment pricing of a merchant
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [AllowAnonymous]
    public async Task<ActionResult<InstallmentPricingResponse>> GetAllAsync([FromQuery] InstallmentPricingRequest request)
    {
        return await _installmentHttpClient.CalcuateInstallmentPricings(request);
    }

    /// <summary>
    /// Returns installments of a merchant for Manual Payment Page
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("ManualPaymentPage/{merchantId}")]
    [Authorize(Policy = "Installment:ReadAll")]
    public async Task<ActionResult<InstallmentsManualPaymentPageResponse>> GetManualPaymentPageInstallmentsAsync([FromRoute] Guid merchantId)
    {
        return await _installmentHttpClient.GetManualPaymentPageInstallments(merchantId);
    }
}