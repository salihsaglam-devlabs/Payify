namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses
{
    public class TransactionSummaryDto
    {
        public string WalletNumber { get; set; }
        public decimal MoneyIn { get; set; }
        public decimal MoneyOut { get; set; }
        public decimal Net { get; set; }
        public CurrencyDto Currency { get; set; }
    }
}
