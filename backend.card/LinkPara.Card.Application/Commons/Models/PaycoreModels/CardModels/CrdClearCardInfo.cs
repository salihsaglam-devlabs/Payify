using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class CrdClearCardInfo
{
    [JsonPropertyName("cardNo")]
    public string CardNo { get; set; }
    [JsonPropertyName("cardToken")]
    public string CardToken { get; set; }
    [JsonPropertyName("cardUniqId")]
    public int CardUniqId { get; set; }
}

