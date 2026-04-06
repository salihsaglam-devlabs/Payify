using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class CompanyPoolsController : ApiControllerBase
{
    private readonly ICompanyPoolHttpClient _companyPoolHttpClient;

    public CompanyPoolsController(ICompanyPoolHttpClient companyPoolHttpClient)
    {
        _companyPoolHttpClient = companyPoolHttpClient;
    }

    /// <summary>
    /// Get CompanyPools
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "CompanyPool:ReadAll")]
    public async Task<PaginatedList<CompanyPoolDto>> GetCompanyPoolsList([FromQuery] GetCompanyPoolListRequest request)
    {
        return await _companyPoolHttpClient.GetCompanyPoolsListAsync(request);
    }

    /// <summary>
    /// Get CompanyPool By Id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CompanyPool:Read")]
    public async Task<CompanyPoolDto> GetCompanyPoolById([FromRoute] Guid id)
    {
        return await _companyPoolHttpClient.GetCompanyPoolAsync(id);
    }

    /// <summary>
    /// Approve Company Pool
    /// </summary>
    /// <returns></returns>
    [HttpPut("approve")]
    [Authorize(Policy = "CompanyPool:Update")]
    public async Task ApproveCompanyPoolAsync([FromBody] ApproveCompanyPoolRequest request)
    {
        await _companyPoolHttpClient.ApproveCompanyPoolAsync(request);
    }

    /// <summary>
    /// save company pool
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "CompanyPool:Create")]
    public async Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync([FromForm] SaveCompanyPoolRequest request)
    {
        return await _companyPoolHttpClient.SaveCompanyPoolAsync(request);
    }    /// <summary>
         /// Get DocumentTypes
         /// </summary>
         /// <returns></returns>
    [HttpGet("document-types")]
    [Authorize(Policy = "CompanyPool:Read")]
    public async Task<List<CompanyDocumentTypeResponse>> GetCompanyDocumentTypesAsync([FromQuery] GetCompanyDocumentTypesRequest request)
    {
        return await _companyPoolHttpClient.GetCompanyDocumentTypesAsync(request);
    }
}
