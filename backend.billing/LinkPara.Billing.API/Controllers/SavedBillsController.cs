using LinkPara.Billing.Application.Features.SavedBills;
using LinkPara.Billing.Application.Features.SavedBills.Commands.CreateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.DeleteSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.UpdateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Queries.GetAllSavedBill;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class SavedBillsController : ApiControllerBase
{
    /// <summary>
    /// get all saved bills
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<SavedBillDto>> GetAllAsync([FromQuery] GetAllSavedBillQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// save new bill
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] CreateSavedBillCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// update saved bill
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Update")]
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] UpdateSavedBillCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// delete saved bill
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Billing:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteSavedBillCommand { Id = id });
    }
}