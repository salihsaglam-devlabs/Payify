using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

public class GetCustomerCardsResponse
{
    [JsonPropertyName("bankingCustomerNo")]
    public string BankingCustomerNo { get; set; }
    [JsonPropertyName("cardNo")]
    public string CardNo { get; set; }
    [JsonPropertyName("mainCardNo")]
    public string MainCardNo { get; set; }
    [JsonPropertyName("physicalType")]
    public string PhysicalType { get; set; }
    [JsonPropertyName("cardLevel")]
    public string CardLevel { get; set; }
    [JsonPropertyName("branchCode")]
    public int BranchCode { get; set; }
    [JsonPropertyName("productName")]
    public string ProductName { get; set; }
    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; }
    [JsonPropertyName("statCode")]
    public string StatCode { get; set; }
    [JsonPropertyName("statDescription")]
    public string StatDescription { get; set; }
    [JsonPropertyName("statusReason")]
    public string StatusReason { get; set; }
    [JsonPropertyName("embossName1")]
    public string EmbossName1 { get; set; }
    [JsonPropertyName("embossName2")]
    public string EmbossName2 { get; set; }
    [JsonPropertyName("isNonameCard")]
    public bool IsNonameCard { get; set; }
    [JsonPropertyName("customerGroupType")]
    public string CustomerGroupType { get; set; }
    [JsonPropertyName("cardBrand")]
    public string CardBrand { get; set; }
}
