using LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Billing;

public class InstitutionsController : ApiControllerBase
{
    private readonly IInstitutionHttpClient _institutionHttpClient;

    public InstitutionsController(IInstitutionHttpClient institutionHttpClient)
    {
        _institutionHttpClient = institutionHttpClient;
    }

    /// <summary>
    /// get all institutions list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Institution:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<InstitutionDto>>> GetAllInstitutionAsync([FromQuery] InstitutionFilterRequest request)
    {
        return await _institutionHttpClient.GetAllInstitutionAsync(request);
    }
}