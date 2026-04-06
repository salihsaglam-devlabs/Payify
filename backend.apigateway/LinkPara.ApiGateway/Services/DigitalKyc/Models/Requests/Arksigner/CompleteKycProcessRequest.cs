namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
public class CompleteKycProcessRequest
{
    public string TransactionId { get; set; }
    public string Version { get; set; }
    public string Model { get; set; }
    public string Brand { get; set; }
    public string OperatingSystem { get; set; }
    public int ChannelId { get; set; }
}
