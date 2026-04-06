using LinkPara.PF.Application.Features.Tokens;
using LinkPara.PF.Application.Features.Tokens.Commands.GenerateCardToken;
using LinkPara.PF.Application.Features.Tokens.Queries.GetCardToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class TokensController : ApiControllerBase
{
    /// <summary>
    /// Generate Card Token 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<CardTokenDto> GenerateCardTokenAsync(GenerateCardTokenCommand command)
    {
        return await Mediator.Send(command);
    }
    
    /// <summary>
    /// Get Card Token 
    /// </summary>
    /// <param name="threeDSessionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "CardToken:Read")]
    [HttpGet("{threeDSessionId}")]
    public async Task<CardTokenDto> GetCardTokenAsync([FromRoute] string threeDSessionId)
    {
        return await Mediator.Send(new GetCardTokenQuery{ ThreeDSessionId = threeDSessionId});
    }
}