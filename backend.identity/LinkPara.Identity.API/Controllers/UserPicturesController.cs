using LinkPara.Identity.Application.Features.Account.Commands.SaveUserPicture;
using LinkPara.Identity.Application.Features.Account.Queries;
using LinkPara.Identity.Application.Features.Account.Queries.GetUserPicture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers
{
    public class UserPicturesController : ApiControllerBase
    {

        /// <summary>
        /// Get user picture.
        /// </summary>
        /// <param name="query"></param>
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<UserPictureDto> GetUserPictureAsync([FromQuery] GetUserPictureQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Upload user picture.
        /// </summary>
        /// <param name="userPicture"></param>
        [Authorize(Policy = "UserPicture:Create")]
        [HttpPost("")]
        public async Task UploadUserPictureAsync(UserPictureDto userPicture)
        {
            var command = new SaveUserPictureCommand() { UserPicture = userPicture };
            await Mediator.Send(command);
        }
    }
}
