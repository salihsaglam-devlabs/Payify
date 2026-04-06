using LinkPara.PF.Application.Commons.Models.CardBins;
using LinkPara.PF.Application.Commons.Models.PricingProfiles;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.CardBins.Command.DeleteCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.PatchCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetAllCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinById;
using LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinByNumber;
using LinkPara.PF.Application.Features.PricingProfiles.Command.PatchPricingProfile;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class CardBinsController : ApiControllerBase
{
    /// <summary>
    /// Returns a card bin.
    /// </summary>
    /// <param name="binNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Read")]
    [HttpGet("{binNumber}")]
    public async Task<ActionResult<CardBinDto>> GetByNumberAsync([FromRoute] string binNumber)
    {
        return await Mediator.Send(new GetCardBinByNumberQuery { BinNumber = binNumber });
    }

    /// <summary>
    /// Returns a card bin.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardBinDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCardBinByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns all card bin.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<CardBinDto>>> GetAllAsync([FromQuery] GetAllCardBinQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new card bin
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveCardBinCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates card bin.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateCardBinCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete card bin.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteCardBinCommand { Id = id });
    }

    /// <summary>
    /// Updates card bin with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cardBin"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardBin:Update")]
    [HttpPatch("{id}")]
    public async Task<UpdateCardBinRequest> PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateCardBinRequest> cardBin)
    {
        return await Mediator.Send(new PatchCardBinCommand { Id = id, CardBin = cardBin });
    }
}
