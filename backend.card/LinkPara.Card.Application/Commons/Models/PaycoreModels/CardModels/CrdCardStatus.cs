using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
public class CrdCardStatus
{
    [JsonPropertyName("cardNo")]
    public string CardNo { get; set; }
    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; set; }
    [JsonPropertyName("statCode")]
    public string StatCode { get; set; }
    [JsonPropertyName("statDescription")]
    public string StatDescription { get; set; }
    [JsonPropertyName("statReasonCode")]
    public string StatReasonCode { get; set; }
    [JsonPropertyName("statReasonDescription")]
    public string StatReasonDescription { get; set; }
}
