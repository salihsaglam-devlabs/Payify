namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.Arksigner;
public class StartKycProcessRequest
{
    public string TransactionId { get; set; }
    public string UserId { get; set; }
    public string IdentityNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Version { get; set; }
    public string Model { get; set; }
    public string Brand { get; set; }
    public string OperatingSystem { get; set; }
    public string DocumentType { get; set; }
    public string IcaoCode { get; set; }
}