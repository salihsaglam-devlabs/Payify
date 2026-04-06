namespace LinkPara.PF.Pos.ApiGateway.Models.Requests;

public class ParameterRequest
{
    public int Date { get; set; }
    public string DeviceSerial { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public List<AppInfo> AppInfo { get; set; }
}

public class AppInfo
{
    public string Name { get; set; }
    public string AcqId { get; set; }
    public string Tid { get; set; }
    public string Mid { get; set; }
    public string State { get; set; }
}

public class ParameterMerchantRequest : ParameterRequest
{
    public ParameterMerchantRequest(ParameterRequest request)
    {
        Date = request.Date;
        DeviceSerial = request.DeviceSerial;
        InstitutionId = request.InstitutionId;
        Vendor = request.Vendor;
        AppInfo = request.AppInfo;
    }
    
    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}