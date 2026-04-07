using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels
{
    public class PaycoreProduct
    {
        [JsonPropertyName("guid")]
        public long Guid { get; set; }
        [JsonPropertyName("dci")]
        public string Dci { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("expiryPeriodMonths")]
        public int ExpiryPeriodMonths { get; set; }
        [JsonPropertyName("segment")]
        public string Segment { get; set; }
        [JsonPropertyName("physicalType")]
        public string PhysicalType { get; set; }
        [JsonPropertyName("plasticType")]
        public string PlasticType { get; set; }
        [JsonPropertyName("virtualCardProductCode")]
        public object VirtualCardProductCode { get; set; }
        [JsonPropertyName("hceCardProductCode")]
        public object HceCardProductCode { get; set; }
        [JsonPropertyName("customerGroups")]
        public object[] CustomerGroups { get; set; }
        [JsonPropertyName("isBusiness")]
        public bool IsBusiness { get; set; }
        [JsonPropertyName("isEmv")]
        public bool IsEmv { get; set; }
        [JsonPropertyName("isContactless")]
        public bool IsContactless { get; set; }
        [JsonPropertyName("isOpen")]
        public bool IsOpen { get; set; }
        [JsonPropertyName("expiryDateType")]
        public string ExpiryDateType { get; set; }
        [JsonPropertyName("hasPhoto")]
        public bool? HasPhoto { get; set; }
        [JsonPropertyName("bin")]
        public object Bin { get; set; }
        [JsonPropertyName("isBrandShared")]
        public object IsBrandShared { get; set; }
    }
}
