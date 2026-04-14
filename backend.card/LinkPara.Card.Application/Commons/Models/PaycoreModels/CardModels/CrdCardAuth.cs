namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class CrdCardAuth
    {
        public bool AuthDomEcom { get; set; }
        public bool AuthDomMoto { get; set; }
        public bool AuthDomNoCVV2 { get; set; }
        public bool AuthDomContactless { get; set; }
        public bool AuthDomCash { get; set; }
        public bool AuthDomCasino { get; set; }
        public bool AuthIntEcom { get; set; }
        public bool AuthIntMoto { get; set; }
        public bool AuthIntNoCVV2 { get; set; }
        public bool AuthIntContactless { get; set; }
        public bool AuthIntCash { get; set; }
        public bool AuthIntCasino { get; set; }
        public bool AuthIntPosSale { get; set; }
        public bool Auth3dSecure { get; set; }
        public string Auth3dSecureType { get; set; }
        public bool AuthCloseInstallment { get; set; }
        public bool VisaAccountUpdaterOpt { get; set; }
    }
}
