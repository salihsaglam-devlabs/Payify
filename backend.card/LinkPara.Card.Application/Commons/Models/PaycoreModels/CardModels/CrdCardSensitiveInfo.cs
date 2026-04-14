using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class CrdCardSensitiveInfo
{
    [JsonPropertyName("cardNo")]
    public string CardNo { get; set; }
    [JsonPropertyName("expiryDateAndCvv2")]
    public string ExpiryDate { get; set; }
    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; set; }
    [JsonPropertyName("bankingCustomerNo")]
    public string BankingCustomerNo { get; set; }
    public string Cvv2 { get; set; }

}

