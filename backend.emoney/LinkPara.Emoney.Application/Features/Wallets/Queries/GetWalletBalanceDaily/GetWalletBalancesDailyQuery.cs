using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalanceDaily
{
    public class GetWalletBalancesDailyQuery : SearchQueryParams, IRequest<WalletBalanceDailyResponse>
    {
        public DateTime? TransactionDate { get; set; }
    }

    public class GetWalletBalancesDailyQueryHandler : IRequestHandler<GetWalletBalancesDailyQuery, WalletBalanceDailyResponse>
    {
        private readonly IWalletService _walletService;

        public GetWalletBalancesDailyQueryHandler(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<WalletBalanceDailyResponse> Handle(GetWalletBalancesDailyQuery query, CancellationToken cancellationToken)
        {
            return await _walletService.GetWalletBalanceDailyAsync(query);
        }
    }
}