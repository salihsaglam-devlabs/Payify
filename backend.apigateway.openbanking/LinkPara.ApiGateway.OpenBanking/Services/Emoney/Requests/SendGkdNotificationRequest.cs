using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;

public class SendGkdNotificationRequest
{
    public byte CustomerType { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }
    public string DecoupledIdType { get; set; }
    public string DecoupledIdValue { get; set; } 
    public string MessageContentTR { get; set; } 
    public string MessageContentEN { get; set; } 
    public string DeepLink { get; set; } 

}
