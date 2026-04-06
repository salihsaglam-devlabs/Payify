using LinkPara.Emoney.Domain.Enums;
using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class GenerateAccessTokenRequest
{
    [JsonIgnore]
    public int MerchantId { get; set; }
    public string OrderNo { get; set; }
    public string UserId { get; set; }
    public string AccountKey { get; set; }
    public bool IsMsisdnValidated { get; set; }
    public AuthenticationMethod AuthenticationMethod { get; set; }
    public Secure3dType Secure3dType { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
}