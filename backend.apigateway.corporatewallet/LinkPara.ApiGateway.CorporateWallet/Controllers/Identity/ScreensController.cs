using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Identity
{
    public class ScreensController : ApiControllerBase
    {
        private readonly IScreenHttpClient _screenHttpClient;

        public ScreensController(IScreenHttpClient screenHttpClient)
        {
            _screenHttpClient = screenHttpClient;
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
        /// Gets the role detail with screens.
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}")]
      //  [Authorize(Policy = "Role:ReadAll")]
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
