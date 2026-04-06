namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckIdentityIsNewRequest
{
    public string SessionId { get; set; }
    public string IdentityNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string UserId { get; set; }
    public string DeviceBrand { get; set; }
    public string DeviceModel { get; set; }
    public string DeviceOs { get; set; }
    public string DeviceOsVersion { get; set; }
}
