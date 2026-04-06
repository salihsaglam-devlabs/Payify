namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels
{
    public class DebitAuthorizationResponse
    {
        public long CorrelationID { get; set; }
        public string BankingRefNo { get; set; }
        public List<BalanceInfo> BalanceInformationList { get; set; }
        public PaycoreAmount TransactionAmount { get; set; }
        public PaycoreAmount BillingAmount { get; set; }
        public List<Fee> FeeList { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string ResponseMessage { get; set; }
        public bool IsApproved { get; set; }
    }
}
