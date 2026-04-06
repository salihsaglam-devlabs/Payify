using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.DigitalKyc;
public class ScSoftController : ApiControllerBase
{
    private readonly IScSoftHttpClient _httpClient;
    public ScSoftController(IScSoftHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns all customer informations
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:ReadAll")]
    [HttpGet("customer-information")]
    public async Task<PaginatedList<CustomerInformationResponse>> GetAllCustomerInformationsAsync
        ([FromQuery] GetAllCustomerInformationsRequest request)
    {
        return await _httpClient.GetAllCustomerInformationsAsync(request);
    }

    /// <summary>
    /// Returns customer information by Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "DigitalKyc:Read")]
    [HttpGet("{id}/customer-information")]
    public async Task<CustomerInformationResponse> GetCustomerInformationByIdAsync([FromRoute] Guid id)
    {
        return await _httpClient.GetCustomerInformationByIdAsync(id);
    }
}
