using System.ComponentModel.DataAnnotations;

using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Identity
{
    public class UserPicturesController : ApiControllerBase
    {
        private readonly IUserPictureHttpClient _userPictureHttpClient;

        public UserPicturesController(IUserPictureHttpClient userPictureHttpClient)
        {
            _userPictureHttpClient = userPictureHttpClient;
        }

        /// <summary>
        /// Get user picture.
        /// </summary>
        /// <param name="userId"></param>
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> GetUserPictureAsync(string userId = null)
        {
            userId ??= UserId;
            var userPicture = await _userPictureHttpClient.GetUserPictureAsync(userId);
            if (userPicture?.Bytes is null)
            {
                return Ok();
            }
            return File(userPicture.Bytes, userPicture.ContentType);
        }

        /// <summary>
        /// Upload user picture.
        /// </summary>
        /// <param name="file"></param>
        [HttpPost("")]
        [RequestSizeLimit(bytes: 500000)]
        [Authorize(Policy = "UserPicture:Create")]
        public async Task UploadUserPictureAsync([Required] IFormFile file)
        {
            if (!file.ContentType.Contains("image"))
            {
                throw new BadImageFormatException();
            }

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var userPicture = new UserPictureDto()
            {
                UserId = UserId,
                ContentType = file.ContentType,
                Bytes = memoryStream.ToArray()
            };

            await _userPictureHttpClient.PostUserPictureAsync(userPicture);
        }

        /// <summary>
        /// Delete user picture.
        /// </summary>
        [HttpDelete("")]
        [Authorize(Policy = "UserPicture:Delete")]
        public async Task DeleteUserPictureAsync()
        {
            var userPicture = new UserPictureDto()
            {
                UserId = UserId,
                ContentType = null,
                Bytes = Array.Empty<byte>()
            };

            await _userPictureHttpClient.PostUserPictureAsync(userPicture);
        }
    }
}
