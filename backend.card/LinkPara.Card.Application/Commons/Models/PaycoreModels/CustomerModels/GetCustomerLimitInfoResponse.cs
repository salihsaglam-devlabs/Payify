using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

public class GetCustomerLimitInfoResponse
{
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    [JsonPropertyName("availableLimit")]
    public int AvailableLimit { get; set; }
    [JsonPropertyName("customerLimit")]
    public int CustomerLimit { get; set; }
    [JsonPropertyName("customerAvailableLimit")]
    public int CustomerAvailableLimit { get; set; }
    [JsonPropertyName("currency")]
    public int Currency { get; set; }
    [JsonPropertyName("isInOverLimit")]
    public bool IsInOverLimit { get; set; }
    [JsonPropertyName("limitExts")]
    public Limitext[] LimitExts { get; set; }
}
public class Limitext
{
    [JsonPropertyName("limitType")]
    public string LimitType { get; set; }
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    [JsonPropertyName("limitRate")]
    public int LimitRate { get; set; }
    [JsonPropertyName("availableLimit")]
    public int AvailableLimit { get; set; }
}