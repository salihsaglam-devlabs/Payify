using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LinkPara.Identity.Application.Features.DeviceInfos;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Identity;
using LinkPara.SharedModels.Persistence;
using LinkPara.Identity.Application.Features.DeviceInfos.Commands.DeleteUserDeviceCommand;

namespace LinkPara.Identity.Infrastructure.Services
{
    public class DeviceInfoService : IDeviceInfoService
    {
        private readonly IRepository<DeviceInfo> _deviceInfoRepository;
        private readonly IRepository<UserDeviceInfo> _userDeviceInfoRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IContextProvider _contextProvider;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IMapper _mapper;

        public DeviceInfoService(IRepository<DeviceInfo> deviceInfo,
            IContextProvider contextProvider,
            IApplicationUserService applicationUserService,
            IMapper mapper, IRepository<User> repository,
            IRepository<UserDeviceInfo> userDeviceInfoRepository)
        {
            _deviceInfoRepository = deviceInfo;
            _contextProvider = contextProvider;
            _applicationUserService = applicationUserService;
            _mapper = mapper;
            _userRepository = repository;
            _userDeviceInfoRepository = userDeviceInfoRepository;
        }

        public async Task<DeviceInfoDto> AddAsync(DeviceInfoDto deviceInfoDto)
        {
            var device = await CreateDevice(deviceInfoDto);
            var contextUser = _contextProvider.CurrentContext.UserId;
            if (contextUser != null)
            {
                var user = _userRepository.GetAll().SingleOrDefault(p => p.Id.ToString() == contextUser);

                if (user is null)
                {
                    throw new NotFoundException(nameof(User));
                }
                await AddUserDeviceAsync(user, device);
            }
            return device;
        }

        private async Task<DeviceInfoDto> CreateDevice(DeviceInfoDto deviceInfoRequest)
        {
            var appUser = _applicationUserService.ApplicationUserId.ToString();
            var previousDevice = await _deviceInfoRepository.GetAll()
                    .FirstOrDefaultAsync(p => p.RegistrationToken == deviceInfoRequest.RegistrationToken);

            var deviceInfo = _mapper.Map<DeviceInfo>(deviceInfoRequest);

            if (previousDevice != null)
            {
                deviceInfo.CreatedBy = previousDevice.CreatedBy;
                deviceInfo.CreateDate = previousDevice.CreateDate;
                deviceInfo.LastModifiedBy = appUser;
                deviceInfo.Id = previousDevice.Id;
                await _deviceInfoRepository.UpdateAsync(deviceInfo);
                deviceInfoRequest.Id = previousDevice.Id;
                return deviceInfoRequest;
            }
            deviceInfo.CreatedBy = appUser;
            var createdDeviceInfo = await _deviceInfoRepository.AddAsync(deviceInfo);
            deviceInfoRequest.Id = createdDeviceInfo.Id;
            return deviceInfoRequest;
        }
        public async Task AddUserDeviceAsync(User user, DeviceInfoDto deviceInfoDto)
        {
            await DisableOldUserDeviceInfo(user, deviceInfoDto);

            var userDevice = _userDeviceInfoRepository.GetAll()
                .Include(i => i.DeviceInfo)
                .SingleOrDefault(p => p.UserId == user.Id
                                      && p.DeviceInfo.DeviceId == deviceInfoDto.DeviceId);

            if (userDevice is not null)
            {
                if (userDevice.DeviceInfo.RegistrationToken != deviceInfoDto.RegistrationToken)
                {
                    var device = userDevice.DeviceInfo;
                    device.RegistrationToken = deviceInfoDto.RegistrationToken;
                    await _deviceInfoRepository.UpdateAsync(device);
                }
                return;
            }

            await _userDeviceInfoRepository.AddAsync(new UserDeviceInfo()
            {
                IsMainDevice = true,
                UserId = user.Id,
                DeviceInfoId = deviceInfoDto.Id,
                CreatedBy = user.Id.ToString(),
            });
        }

        private async Task DisableOldUserDeviceInfo(User user, DeviceInfoDto deviceInfoDto)
        {
            var oldUserDevices = _userDeviceInfoRepository
                .GetAll()
                .Include(i => i.DeviceInfo)
                .Where(p =>
                    p.UserId != user.Id
                    && p.DeviceInfo.DeviceId == deviceInfoDto.DeviceId);

            foreach (var userDevice in oldUserDevices)
            {
                userDevice.RecordStatus = RecordStatus.Passive;
                userDevice.IsMainDevice = false;
                await _userDeviceInfoRepository.UpdateAsync(userDevice);
            }
        }

        public async Task<List<UserDeviceInfoDto>> GetUsersDeviceInfoAsync(List<Guid> userIdList)
        {
            var userDeviceList = await _userDeviceInfoRepository.GetAll().Include(i => i.DeviceInfo)
                .Where(p =>
                    userIdList.Contains(p.UserId)
                    && p.IsMainDevice
                    && p.RecordStatus == RecordStatus.Active)
                .ToListAsync();
            var dtoList = _mapper.Map<List<UserDeviceInfoDto>>(userDeviceList);
            return dtoList;
        }

        public async Task DeleteUserDeviceAsync(DeleteUserDeviceCommand deleteUserDeviceCommand)
        {

            var deviceInfo = await _deviceInfoRepository.GetAll()
                .FirstOrDefaultAsync(di => di.DeviceId == deleteUserDeviceCommand.DeviceId);

            if (deviceInfo == null)
            {
                throw new NotFoundException("DeviceInfo not found.");
            }

            var userDevice = await _userDeviceInfoRepository.GetAll()
                .FirstOrDefaultAsync(ud => ud.DeviceInfoId == deviceInfo.Id);

            if (userDevice == null)
            {
                throw new NotFoundException("UserDevice not found.");
            }

            userDevice.IsMainDevice = false;
            userDevice.RecordStatus = RecordStatus.Passive;
            await _userDeviceInfoRepository.UpdateAsync(userDevice);
            deviceInfo.RecordStatus = RecordStatus.Passive;
            await _deviceInfoRepository.UpdateAsync(deviceInfo);
        }
    }
}