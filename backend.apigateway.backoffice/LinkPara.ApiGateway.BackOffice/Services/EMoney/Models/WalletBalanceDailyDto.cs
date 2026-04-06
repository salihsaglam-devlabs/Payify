using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models
{
    public class WalletBalanceDailyDto
    {
        public DateTime JobDate { get; set; }
        public decimal DailyBalance { get; set; }
        public string Currency { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Difference { get; set; }
    }
}