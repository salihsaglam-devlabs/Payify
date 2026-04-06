using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.PricingModels;

public class TodebDeclarationRequest
{
    [JsonPropertyName("tcknListesi")]
    public List<string> IdentityNumberList { get; set; }
}
