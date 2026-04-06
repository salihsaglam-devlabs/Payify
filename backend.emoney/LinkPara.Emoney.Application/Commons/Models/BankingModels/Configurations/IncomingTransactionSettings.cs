using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.BankingModels.Configurations;

public class IncomingTransactionSettings
{
    [JsonPropertyName("ExemptTiersForKkbValidation")]
    public List<string> ExemptTiersForKkbValidation { get; set; }
}
