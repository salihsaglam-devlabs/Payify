using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities
{
    public class WalletBalanceDaily : AuditEntity
    {
        public DateTime JobDate { get; set; }
        public decimal DailyBalance { get; set; }
        public string Currency { get; set; }
        public decimal Difference { get; set; }
    }
}