using LinkPara.Identity.Application.Features.SecurityPictures.Commands.CreateSecurityPicture;
using LinkPara.Identity.Application.Features.SecurityPictures.Commands.DeleteSecurityPicture;
using LinkPara.Identity.Application.Features.SecurityPictures.Queries;
using LinkPara.Identity.Application.Features.SecurityPictures.Queries.GetAllSecurityPicture;
using LinkPara.Identity.Application.Features.SecurityPictures.Queries.GetSecurityPictureById;
using LinkPara.Identity.Application.Features.UserSecurityPictures.Commands.CreateUserSecurityPicture;
using LinkPara.Identity.Application.Features.UserSecurityPictures.Commands.ResetUserSecurityPicture;
using LinkPara.Identity.Application.Features.UserSecurityPictures.Queries.GetUserSecurityPicture;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class SecurityPicturesController : ApiControllerBase
{
    /// <summary>
    /// Returns all active security pictures (for user selection screen).
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<SecurityPictureDto>>> GetAllAsync([FromQuery] GetAllSecurityPictureQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a security picture by id.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SecurityPictureDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSecurityPictureByIdQuery { Id = id });
    }

    /// <summary>
    /// Creates a new security picture (backoffice).
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult> SaveAsync(CreateSecurityPictureCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a security picture (backoffice).
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteSecurityPictureCommand { Id = id });
    }

    /// <summary>
    /// Returns the security picture selected by the user (shown during OTP step).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{userId}/picture")]
    public async Task<ActionResult<SecurityPictureDto>> GetUserSecurityPictureAsync([FromRoute] Guid userId)
    {
        return await Mediator.Send(new GetUserSecurityPictureQuery { UserId = userId });
    }

    /// <summary>
    /// Saves or updates the security picture selected by the user.
    /// </summary>
    [Authorize]
    [HttpPost("{pictureId}/select")]
    public async Task<ActionResult> SelectPictureAsync(CreateUserSecurityPictureCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Resets (removes) the security picture of a user (admin only).
    /// </summary>
    [Authorize(Policy = "User:Update")]
    [HttpDelete("{userId}/picture")]
    public async Task<ActionResult> ResetUserSecurityPictureAsync([FromRoute] Guid userId)
    {
        await Mediator.Send(new ResetUserSecurityPictureCommand { UserId = userId });
        return NoContent();
    }
}
