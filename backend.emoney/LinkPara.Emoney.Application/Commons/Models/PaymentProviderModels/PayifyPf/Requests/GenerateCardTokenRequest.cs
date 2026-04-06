using LinkPara.Emoney.Domain.Enums;
using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class GenerateCardTokenRequest
{
    public string CardNumber { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string Cvv { get; set; }
    [JsonIgnore]
    public PaymentProviderType PaymentProviderType { get; set; }
}