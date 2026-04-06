using System.Text.Json.Serialization;

namespace LinkPara.PF.Application.Commons.Models.VposModels.VposShortCircuit;

public class ShortCircuitRequestModel
{
    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; }
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; }
    [JsonPropertyName("installment_count")]
    public int InstallmentCount { get; set; }
    [JsonPropertyName("ruleset_id")]
    public string RulesetId { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}