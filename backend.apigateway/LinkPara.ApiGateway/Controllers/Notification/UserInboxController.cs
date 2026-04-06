using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Notification
{
    public class UserInboxController : ApiControllerBase
    {
        private readonly IUserInboxHttpClient _userInboxHttpClient;

        public UserInboxController(IUserInboxHttpClient userInboxHttpClient, 
            IUserNameGenerator userNameGenerator, 
            IUserHttpClient client)
        {
            _userInboxHttpClient = userInboxHttpClient;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<List<UserInboxDto>> GetUserInboxAsync([FromQuery] UserInboxRequest req)
        {
            return await _userInboxHttpClient.GetUserInboxAsync(req);
        }

        [Authorize]
        [HttpPut("read-all")]
        public async Task UpdateReadedUserInboxAsync([FromBody]UserInboxRequest req)
        {
            await _userInboxHttpClient.UpdateReadedUserInboxAsync(req);
        }

        [Authorize]
        [HttpPatch("delete-selected")]
        public async Task<IActionResult> DeleteSelectedAsync([FromBody] DeleteUserInboxRequest request)
        {
            await _userInboxHttpClient.DeleteSelectedAsync(request);
            return NoContent();
        }
    }
}