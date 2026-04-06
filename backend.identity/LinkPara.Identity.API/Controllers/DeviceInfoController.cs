using LinkPara.Identity.Application.Common.Models.DeviceInfo;
using LinkPara.Identity.Application.Features.DeviceInfos;
using LinkPara.Identity.Application.Features.DeviceInfos.Commands.CreateDeviceInfo;
using LinkPara.Identity.Application.Features.DeviceInfos.Commands.DeleteUserDeviceCommand;
using LinkPara.Identity.Application.Features.DeviceInfos.Commands.GetUserDeviceInfoCommand;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers
{
    public class DeviceInfoController : ApiControllerBase
    {
        /// <summary>
        /// Crete device.
        /// </summary>
        /// <param name="deviceInfo"></param>
        [AllowAnonymous]
        [HttpPost("")]
        public async Task CreateAsync(CreateDeviceInfoCommand deviceInfo)
        {
            await Mediator.Send(deviceInfo);
        }

        /// <summary>
        /// Get user device.
        /// </summary>
        /// <param name="userIdList"></param>
        [Authorize(Policy = "UserDevice:Read")]
        [HttpPost("user-device")]
        public async Task<List<UserDeviceInfoDto>> GetUserDeviceInfo(GetUserDeviceInfoCommand getUserDeviceInfoCommand)
        {
            return await Mediator.Send(getUserDeviceInfoCommand);
        }

        /// <summary>
        /// Delete user device.
        /// </summary>
        /// <param name="deviceId"></param>
        [AllowAnonymous]
        [HttpPost("delete")]
        public async Task DeleteDevice(DeleteUserDeviceCommand deleteUserDeviceCommand)
        {
            await Mediator.Send(deleteUserDeviceCommand);
        }
    }
}
