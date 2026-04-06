using LinkPara.PF.Application.Features.MerchantContents;
using LinkPara.PF.Application.Features.MerchantContents.Command.CreateMerchantContent;
using LinkPara.PF.Application.Features.MerchantContents.Command.DeleteMerchantContentCommand;
using LinkPara.PF.Application.Features.MerchantContents.Command.PutMerchantContent;
using LinkPara.PF.Application.Features.MerchantContents.Command.UploadMerchantLogo;
using LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantContentById;
using LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantContentsQuery;
using LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantLogo;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantContentsController : ApiControllerBase
{
    /// <summary>
    /// Returns merchant contents
    /// </summary>/
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantContentDto>> GetMerchantContentsAsync([FromQuery]GetMerchantContentsQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Returns merchant content by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Read")]
    [HttpGet("{id}")]
    public async Task<MerchantContentDto> GetMerchantContentByIdAsync(Guid id)
    {
        return await Mediator.Send(new GetMerchantContentByIdQuery { Id = id });
    }
    
    /// <summary>
    /// Create a new merchant content
    /// </summary>
    /// <param name="command"></param>
    [Authorize(Policy = "MerchantContent:Create")]
    [HttpPost]
    public async Task CreateMerchantContentAsync(CreateMerchantContentCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Updates merchant content
    /// </summary>
    /// <param name="merchantContent"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Update")]
    [HttpPut]
    public async Task PutMerchantContentAsync(MerchantContentDto merchantContent)
    {
        await Mediator.Send(new PutMerchantContentCommand { MerchantContent = merchantContent });
    }
    
    /// <summary>
    /// Deletes merchant content
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantContent:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteMerchantContentAsync(Guid id)
    {
        await Mediator.Send(new DeleteMerchantContentCommand { Id = id });
    }
    
    /// <summary>
    /// Get merchant logo.
    /// </summary>
    /// <param name="merchantId"></param>
    [Authorize(Policy = "MerchantContent:Read")]
    [HttpGet("logo/{merchantId}")]
    public async Task<MerchantLogoDto> GetMerchantLogoAsync(Guid merchantId)
    {
        return await Mediator.Send(new GetMerchantLogoQuery{MerchantId = merchantId});
    }
    
    /// <summary>
    /// Upload merchant logo.
    /// </summary>
    /// <param name="merchantLogo"></param>
    [Authorize(Policy = "MerchantContent:Create")]
    [HttpPost("logo")]
    public async Task UploadMerchantLogoAsync(MerchantLogoDto merchantLogo)
    {
        var command = new UploadMerchantLogoCommand { MerchantLogo = merchantLogo };
        await Mediator.Send(command);
    }
}