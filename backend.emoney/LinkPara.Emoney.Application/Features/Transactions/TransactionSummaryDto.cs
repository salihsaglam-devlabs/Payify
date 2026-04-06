

using LinkPara.Emoney.Application.Features.Currencies;

namespace LinkPara.Emoney.Application.Features.Transactions
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
