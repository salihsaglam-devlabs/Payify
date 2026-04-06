using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class AccountUnlinkRequest
{
    [JsonIgnore]
    public int MerchantId { get; set; }
    public string AccountKey { get; set; }
    public string UserId { get; set; }
    public string OrderNo { get; set; }
}