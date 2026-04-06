using LinkPara.Accounting.Application.Features.Customers;
using LinkPara.Accounting.Application.Features.Customers.Commands.SaveCustomer;
using LinkPara.Accounting.Application.Features.Customers.Queries.GetById;
using LinkPara.Accounting.Application.Features.Customers.Queries.GetFilterCustomer;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Accounting.API.Controllers;

public class CustomersController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered Csutomers
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingCustomer:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<CustomerDto>>> GetFilterAsync([FromQuery] GetFilterCustomerQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingCustomer:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetByIdQuery { Id = id });
    }

    /// <summary>
    /// Save a customer
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountingCustomer:Create")]
    [HttpPost("")]
    public async Task<ActionResult<Unit>> SaveCustomerAsync(SaveCustomerCommand request)
    {
        return await Mediator.Send(request);
    }
}
