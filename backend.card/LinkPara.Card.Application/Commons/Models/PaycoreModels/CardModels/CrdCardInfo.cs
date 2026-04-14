using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class CrdCardInfo
{
    [JsonPropertyName("applicationRefNo")]
    public string ApplicationRefNo { get; set; }
    [JsonPropertyName("bankingCustomerNo")]
    public string BankingCustomerNo { get; set; }
    [JsonPropertyName("barcodeNo")]
    public string BarcodeNo { get; set; }
    [JsonPropertyName("batchBarcodeNo")]
    public string BatchBarcodeNo { get; set; }
    [JsonPropertyName("cardNo")]
    public string CardNo { get; set; }
    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; set; }
    [JsonPropertyName("customerType")]
    public string CustomerType { get; set; }
    [JsonPropertyName("dci")]
    public string Dci { get; set; }
    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; }
    [JsonPropertyName("segment")]
    public string Segment { get; set; }
}

