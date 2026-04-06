namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels
{
    public class PaycoreProduct
    {
        public long guid { get; set; }
        public string dci { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public int expiryPeriodMonths { get; set; }
        public string segment { get; set; }
        public string physicalType { get; set; }
        public string plasticType { get; set; }
        public object virtualCardProductCode { get; set; }
        public object hceCardProductCode { get; set; }
        public object[] customerGroups { get; set; }
        public bool isBusiness { get; set; }
        public bool isEmv { get; set; }
        public bool isContactless { get; set; }
        public bool isOpen { get; set; }
        public string expiryDateType { get; set; }
        public bool? hasPhoto { get; set; }
        public object bin { get; set; }
        public object isBrandShared { get; set; }
    }
}
