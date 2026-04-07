using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class CrdCardAuth
    {
        [JsonPropertyName("authDomEcom")]
        public bool AuthDomEcom { get; set; }
        [JsonPropertyName("authDomMoto")]
        public bool AuthDomMoto { get; set; }
        [JsonPropertyName("authDomNoCVV2")]
        public bool AuthDomNoCVV2 { get; set; }
        [JsonPropertyName("authDomContactless")]
        public bool AuthDomContactless { get; set; }
        [JsonPropertyName("authDomCash")]
        public bool AuthDomCash { get; set; }
        [JsonPropertyName("authDomCasino")]
        public bool AuthDomCasino { get; set; }
        [JsonPropertyName("authIntEcom")]
        public bool AuthIntEcom { get; set; }
        [JsonPropertyName("authIntMoto")]
        public bool AuthIntMoto { get; set; }
        [JsonPropertyName("authIntNoCVV2")]
        public bool AuthIntNoCVV2 { get; set; }
        [JsonPropertyName("authIntContactless")]
        public bool AuthIntContactless { get; set; }
        [JsonPropertyName("authIntCash")]
        public bool AuthIntCash { get; set; }
        [JsonPropertyName("authIntCasino")]
        public bool AuthIntCasino { get; set; }
        [JsonPropertyName("authIntPosSale")]
        public bool AuthIntPosSale { get; set; }
        [JsonPropertyName("auth3dSecure")]
        public bool Auth3dSecure { get; set; }
        [JsonPropertyName("auth3dSecureType")]
        public string Auth3dSecureType { get; set; }
        [JsonPropertyName("authCloseInstallment")]
        public bool AuthCloseInstallment { get; set; }
        [JsonPropertyName("visaAccountUpdaterOpt")]
        public bool VisaAccountUpdaterOpt { get; set; }
    }
}
