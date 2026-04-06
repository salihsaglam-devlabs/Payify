using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Queries.GetCharges;

public class GetChargeTransactionsSearchQuery : SearchQueryParams, IRequest<PaginatedList<ChargeTransactionDto>>
{
    public string FullName { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public ChargeTransactionType? ChargeTransactionType { get; set; }
    public SourceCampaignType? SourceCampaignType { get; set; }
}
public class GetChargeTransactionsSearchQueryHandler : IRequestHandler<GetChargeTransactionsSearchQuery, PaginatedList<ChargeTransactionDto>>
{
    private readonly IIWalletChargeService _chargeService;

    public GetChargeTransactionsSearchQueryHandler(IIWalletChargeService chargeService)
    {
        _chargeService = chargeService;
    }

    public async Task<PaginatedList<ChargeTransactionDto>> Handle(GetChargeTransactionsSearchQuery request, CancellationToken cancellationToken)
    {
        return await _chargeService.GetChargeTransactionsAsync(request);
    }
}
