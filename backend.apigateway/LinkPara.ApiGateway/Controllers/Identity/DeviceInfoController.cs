using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Identity
{
    public class DeviceInfoController : ApiControllerBase
    {
        private readonly IDeviceInfoHttpClient _deviceInfoHttpClient;
        public DeviceInfoController(IDeviceInfoHttpClient deviceInfoHttpClient)
        {
            _deviceInfoHttpClient = deviceInfoHttpClient;
        }

        [AllowAnonymous]
        [HttpPost("init")]
        public async Task CreateAnonymousAsync(CreateDeviceInfoRequest createDeviceInfoRequest)
        {
            await _deviceInfoHttpClient.CreateAsync(createDeviceInfoRequest);
        }

        [Authorize]
        [HttpPost("")]
        public async Task CreateAsync(CreateDeviceInfoRequest createDeviceInfoRequest)
        {
            await _deviceInfoHttpClient.CreateAsync(createDeviceInfoRequest);
        }

        [AllowAnonymous]
        [HttpPost("delete-device")]
        public async Task DeleteAsync(DeleteDeviceRequest deleteDeviceRequest)
        {
            await _deviceInfoHttpClient.DeleteAsync(deleteDeviceRequest);
        }
    }
}