using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.Emoney.Application.Features.Wallets.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Models.WalletModels
{
    public class WalletBalanceDailyResponse
    {
        public PaginatedList<WalletBalanceDaily> WalletBalances { get; set; }
    }
}