namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests
{
    public class TransactionSummaryRequest
    {
        public string WalletNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CurrencyCode { get; set; }
    }
}
