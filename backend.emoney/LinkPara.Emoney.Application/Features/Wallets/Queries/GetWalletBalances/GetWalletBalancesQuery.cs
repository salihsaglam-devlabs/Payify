using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalances;

public class GetWalletBalancesQuery : SearchQueryParams, IRequest<WalletBalanceResponse>
{
    public string AccountName { get; set; }
    public Guid? AccountId { get; set; }
    public string CurrencyCode { get; set; }
    public string WalletNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? TransactionDate { get; set; }
    public WalletBlockageStatus? BlockageStatus { get; set; }
}

public class GetWalletBalancesQueryHandler : IRequestHandler<GetWalletBalancesQuery, WalletBalanceResponse>
{
    private readonly IWalletService _walletService;

    public GetWalletBalancesQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<WalletBalanceResponse> Handle(GetWalletBalancesQuery query, CancellationToken cancellationToken)
    {
        return await _walletService.GetWalletBalancesAsync(query);
    }
}
