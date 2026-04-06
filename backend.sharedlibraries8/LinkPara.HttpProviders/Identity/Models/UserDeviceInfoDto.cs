namespace LinkPara.HttpProviders.Identity.Models;

public class UserDeviceInfoDto 
{
    public Guid UserId { get; set; }
    public bool IsMainDevice { get; set; }
    public Guid DeviceInfoId { get; set; }
    public DeviceInfoDto DeviceInfo { get; set; }
}