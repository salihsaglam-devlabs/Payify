using LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Accounting;

public class CustomersController : ApiControllerBase
{
    private readonly ICustomerHttpClient _customerHttpClient;

    public CustomersController(ICustomerHttpClient customerHttpClient)
    {
        _customerHttpClient = customerHttpClient;
    }

    /// <summary>
    /// Returns filtered Csutomers
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "AccountingCustomer:ReadAll")]
    public async Task<ActionResult<PaginatedList<AccountingCustomerDto>>> GetFilterAsync([FromQuery] GetFilterCustomerRequest request)
    {
        return await _customerHttpClient.GetListCustomersAsync(request);
    }

    /// <summary>
    /// Returns a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AccountingCustomer:Read")]
    public async Task<ActionResult<AccountingCustomerDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _customerHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns a customer
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "AccountingCustomer:Create")]
    public async Task SaveCustomerAsync(SaveCustomerRequest request)
    {
        await _customerHttpClient.SaveCustomerAsync(request);
    }
}
