using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class CompanyPoolsController : ApiControllerBase
{
    private readonly ICompanyPoolHttpClient _httpClient;

    public CompanyPoolsController(ICompanyPoolHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// save company pool
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "RequireOtp")]
    public async Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync([FromForm] SaveCompanyPoolRequest request)
    {
        return await _httpClient.SaveCompanyPoolAsync(request);
    }

    /// <summary>
    /// Get Tax Administrations
    /// </summary>
    /// <returns></returns>
    [HttpGet("tax-administrations")]
    [AllowAnonymous]
    public async Task<List<TaxAdministrationsResponse>> GetTaxAdministrationsAsync()
    {
        return await _httpClient.GetTaxAdministrationsAsync();
    }

    /// <summary>
    /// Get DocumentTypes
    /// </summary>
    /// <returns></returns>
    [HttpGet("document-types")]
    [AllowAnonymous]
    public async Task<List<CompanyDocumentTypeResponse>> GetCompanyDocumentTypesAsync([FromQuery] GetCompanyDocumentTypesRequest request)
    {
        return await _httpClient.GetCompanyDocumentTypesAsync(request);
    }
}
