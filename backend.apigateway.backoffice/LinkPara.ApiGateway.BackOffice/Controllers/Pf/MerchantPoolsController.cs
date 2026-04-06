using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantPoolsController : ApiControllerBase
{
    private readonly IMerchantPoolHttpClient _merchantPoolHttpClient;
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public MerchantPoolsController(IMerchantPoolHttpClient merchantPoolHttpClient,
        IServiceRequestConverter serviceRequestConverter)
    {
        _merchantPoolHttpClient = merchantPoolHttpClient;
        _serviceRequestConverter = serviceRequestConverter;
    }

    /// <summary>
    /// Returns a merchant that make an application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "MerchantPool:Read")]
    public async Task<ActionResult<MerchantPoolDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantPoolHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns filtered merchant that make an application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantPool:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantPoolDto>>> GetFilterAsync(
        [FromQuery] GetFilterMerchantPoolRequest request)
    {
        return await _merchantPoolHttpClient.GetFilterListAsync(request);
    }

    /// <summary>
    /// Creates a merchant that make an application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "MerchantPool:Create")]
    public async Task SaveAsync(SaveMerchantPoolRequest request)
    {
        await _merchantPoolHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Performs the approval/rejection process for the merchant that make an application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantPool:Update")]
    public async Task<ApproveMerchantPoolResponse> ApproveAsync(ApproveMerchantPoolRequest request)
    {
        var clientRequest =
            _serviceRequestConverter.Convert<ApproveMerchantPoolRequest, ApproveMerchantPoolServiceRequest>(request);

        return await _merchantPoolHttpClient.ApproveAsync(clientRequest);
    }
}