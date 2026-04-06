using LinkPara.PF.Application.Commons.Models.CardBins;
using LinkPara.PF.Application.Commons.Models.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.CardBins.Command.PatchCardBin;
using LinkPara.PF.Application.Features.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.PatchMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetAllMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccByCode;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantCategoryCodesController : ApiControllerBase
{
    /// <summary>
    /// Returns all mcc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MccDto>> GetAllAsync([FromQuery] GetAllMccQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Returns a mcc by code.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Read")]
    [HttpGet("{code}")]
    public async Task<ActionResult<MccDto>> GetByCodeAsync([FromRoute] string code)
    {
        return await Mediator.Send(new GetMccByCodeQuery { Code = code });
    }

    /// <summary>
    /// Returns a mcc.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MccDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMccByIdQuery { Id = id });
    }

    /// <summary>
    /// Creates a mcc.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMccCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a mcc.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMccCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete a mcc.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteMccCommand { Id = id });
    }

    /// <summary>
    /// Updates mcc with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mcc"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantCategoryCode:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdateMccRequest> PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateMccRequest> mcc)
    {
        return await Mediator.Send(new PatchMccCommand { Id = id, Mcc = mcc });
    }
}
