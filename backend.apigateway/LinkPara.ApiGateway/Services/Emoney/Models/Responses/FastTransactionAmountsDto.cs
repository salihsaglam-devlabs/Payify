using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses
{
    public class FastTransactionAmountsDto
    {
        public decimal MostTransactionAmount { get; set; }
        public decimal LastTransactionAmount { get; set; }
        public decimal UserBalanceAmount { get; set; }
    }
}