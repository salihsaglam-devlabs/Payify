using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class CreateDeviceInfoRequest
    {
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceName { get; set; }
        public string RegistrationToken { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string OperatingSystem { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string ScreenResolution { get; set; }
        public string AppVersion { get; set; }
        public string AppBuildNumber { get; set; }
        public string Camera { get; set; }
        
    }
}
