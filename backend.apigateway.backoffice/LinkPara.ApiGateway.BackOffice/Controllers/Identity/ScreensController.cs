using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity
{
    public class ScreensController : ApiControllerBase
    {
        private readonly IScreenHttpClient _screenHttpClient;
        private readonly IUserHttpClient _userHttpClient;

        public ScreensController(IScreenHttpClient screenHttpClient, IUserHttpClient userHttpClient)
        {
            _screenHttpClient = screenHttpClient;
            _userHttpClient = userHttpClient;
        }

        /// <summary>
        /// Lists all created screens.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = "Role:ReadAll")]
        public async Task<ActionResult<List<ScreenDto>>> GetAllScreensAsync([FromQuery] GetRoleScreensRequest request)
        {
            return await _screenHttpClient.GetAllScreensAsync(request);
        }

        /// <summary>
        /// Gets the menu role detail with screens.
        /// </summary>
        /// <returns></returns>
        [HttpGet("role-menu")]
        [Authorize(Policy = "Role:ReadAll")]
        public async Task<ActionResult<List<ScreenDto>>> GetURoleScreensAsync()
        {
            if (!Guid.TryParse(UserId, out Guid userGuidId))
            {
                throw new NotFoundException(nameof(User), UserId!);
            }

            var user = await _userHttpClient.GetUserByIdAsync(userGuidId);

            if (user is null)
            {
                throw new NotFoundException(nameof(User));
            }

            return await _screenHttpClient.GetUserRoleScreenAsync(user.Id.ToString());
        }

        /// <summary>
        /// Gets the role detail with screens.
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}")]
        [Authorize(Policy = "Role:ReadAll")]
        public async Task<ActionResult<RoleScreenDto>> GetRoleScreenAsync(string roleId)
        {
            return await _screenHttpClient.GetRoleScreenAsync(roleId);
        }

        /// <summary>
        /// Update a screen role.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{roleId}")]
        [Authorize(Policy = "Role:Update")]
        public async Task UpdateRoleScreenAsync([FromRoute] string roleId, UpdateRoleScreenRequest request)
        {
            await _screenHttpClient.UpdateRoleScreenAsync(new UpdateRoleScreenRequest()
            {
                ScreensId = request.ScreensId,
                RoleId = roleId.ToString(),
                RoleScope = request.RoleScope,
                Name = request.Name,
                CanSeeSensitiveData = request.CanSeeSensitiveData
            });
        }
        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Policy = "Role:Create")]
        public async Task CreateRoleScreenAsync(RoleScreenRequest request)
        {
            await _screenHttpClient.CreateRoleScreenAsync(request);
        }
        /// <summary>
        /// Removes created role and screens.
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpDelete("{roleId}")]
      //  [Authorize(Policy = "Role:Delete")]
        public async Task DeleteRoleAsync(string roleId)
        {
            await _screenHttpClient.DeleteRoleScreenAsync(roleId);
        }
    }
}
