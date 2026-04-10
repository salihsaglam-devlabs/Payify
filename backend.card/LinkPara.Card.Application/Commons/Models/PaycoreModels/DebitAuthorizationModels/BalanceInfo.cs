using System.Numerics;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels
{
    public class BalanceInfo
    {
        public string Type { get; set; }
        public decimal PreviousAmount { get; set; }
        public int CurrencyCode { get; set; }
        public decimal CurrentAmount { get; set; }  // updatebalance dan dönen current amount
        public long TransactionAmount { get; set; }
    }
}
