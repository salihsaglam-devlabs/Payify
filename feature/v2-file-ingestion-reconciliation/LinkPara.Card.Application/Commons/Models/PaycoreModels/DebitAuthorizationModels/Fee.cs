namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels
{
    public class Fee
    {
        public string Guid { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyCode { get; set; }
        public decimal Tax1Amount { get; set; }
        public decimal Tax2Amount { get; set; }
    }
}
