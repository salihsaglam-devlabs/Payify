using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class CardBinsController : ApiControllerBase
{
    private readonly ICardBinHttpClient _cardBinHttpClient;

    public CardBinsController(ICardBinHttpClient cardBinHttpClient)
    {
        _cardBinHttpClient = cardBinHttpClient;
    }
    /// <summary>
    /// Returns a card bin.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CardBin:Read")]
    public async Task<ActionResult<CardBinDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _cardBinHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns all card bin.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "CardBin:ReadAll")]
    public async Task<ActionResult<PaginatedList<CardBinDto>>> GetAllAsync(
       [FromQuery] GetAllCardBinRequest request)
    {
        return await _cardBinHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Creates a new card bin
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "CardBin:Create")]
    public async Task SaveAsync(SaveCardBinRequest request)
    {
        await _cardBinHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates card bin.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "CardBin:Update")]
    public async Task UpdateAsync(UpdateCardBinRequest request)
    {
        await _cardBinHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete card bin.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "CardBin:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _cardBinHttpClient.DeleteCardBinAsync(id);
    }

    /// <summary>
    /// Updates card bin with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "CardBin:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateCardBinRequest> request)
    {
        await _cardBinHttpClient.PatchAsync(id, request);
    }
}