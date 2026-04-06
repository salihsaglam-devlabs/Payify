using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.DeleteMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.SaveMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.UpdateMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetAllMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetMerchantPreApplicationById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantPreApplicationController : ApiControllerBase
{
    /// <summary>
    /// Returns a merchant pre application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "PreApplication:Read")]
    public async Task<ActionResult<MerchantPreApplicationDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantPreApplicationByIdQuery{Id = id});
    }

    /// <summary>
    /// Returns filtered merchant pre applications
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PreApplication:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantPreApplicationDto>>> GetFilterAsync(
        [FromQuery] GetAllMerchantPreApplicationQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Updates a merchant pre application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "PreApplication:Update")]
    public async Task UpdateAsync(UpdateMerchantPreApplicationCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Delete merchant pre application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "PreApplication:Delete")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteMerchantPreApplicationCommand { Id = id });
    }
    
    /// <summary>
    /// Create a merchant pre application
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [AllowAnonymous]
    public async Task<MerchantPreApplicationCreateResponse> CreateAsync(SaveMerchantPreApplicationCommand request)
    {
        return await Mediator.Send(request);
    }
}