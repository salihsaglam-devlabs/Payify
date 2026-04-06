namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class ThreedSecureRequest
{
    public string MdStatus { get; set; }
    public string MdErrorMsg { get; set; }
    public string Token { get; set; }
    public string Oid { get; set; }
    public string Cavv { get; set; }
    public string Eci { get; set; }
    public string Xid { get; set; }
    public string Hash { get; set; }
    public string TransactionDate { get; set; }
}
