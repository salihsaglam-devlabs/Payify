using LinkPara.PF.Application.Features.MerchantDues;
using LinkPara.PF.Application.Features.MerchantDues.Command.DeleteMerchantDue;
using LinkPara.PF.Application.Features.MerchantDues.Command.SaveMerchantDue;
using LinkPara.PF.Application.Features.MerchantDues.Command.UpdateMerchantDue;
using LinkPara.PF.Application.Features.MerchantDues.Queries.GetFilterMerchantDue;
using LinkPara.PF.Application.Features.MerchantDues.Queries.GetMerchantDueById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantDueController : ApiControllerBase
{
    
    /// <summary>
    /// Returns a merchant due
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Read")]
    [HttpGet("{id}")]
    public async Task<MerchantDueDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantDueByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns filtered merchant dues
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantDueDto>> GetFilterAsync([FromQuery] GetFilterMerchantDueQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Creates a new merchant due
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Create")]
    [HttpPost("")]
    public async Task SaveMerchantDueAsync(SaveMerchantDueCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Updates merchant due
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMerchantDueCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete merchant due
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantDue:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteMerchantDueCommand { Id = id });
    }
}