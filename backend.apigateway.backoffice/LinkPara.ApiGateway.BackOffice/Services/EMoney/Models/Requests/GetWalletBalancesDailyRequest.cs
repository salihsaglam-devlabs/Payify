using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class GetWalletBalancesDailyRequest : SearchQueryParams
    {
        public DateTime? TransactionDate { get; set; }
    }
}