using LinkPara.Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkPara.Identity.Application.Features.DeviceInfos;
using LinkPara.Identity.Application.Features.DeviceInfos.Commands.DeleteUserDeviceCommand;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public interface IDeviceInfoService
    {
        Task<DeviceInfoDto> AddAsync(DeviceInfoDto deviceInfoDto);
        Task AddUserDeviceAsync(User user, DeviceInfoDto deviceInfoDto);
        Task<List<UserDeviceInfoDto>> GetUsersDeviceInfoAsync(List<Guid> userIdList);
        Task DeleteUserDeviceAsync(DeleteUserDeviceCommand deleteUserDeviceCommand);

    }
}