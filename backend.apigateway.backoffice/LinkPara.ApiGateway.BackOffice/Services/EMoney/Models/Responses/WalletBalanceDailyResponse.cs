using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses
{
    public class WalletBalanceDailyResponse
    {
        public PaginatedList<WalletBalanceDailyDto> WalletBalances { get; set; }
    }
}