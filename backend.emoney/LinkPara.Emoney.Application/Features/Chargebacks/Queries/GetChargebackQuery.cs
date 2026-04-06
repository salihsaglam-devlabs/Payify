using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Queries;

public class GetChargebackQuery : SearchQueryParams, IRequest<PaginatedList<ChargebackDto>>
{
    public Guid? TransactionId { get; set; }
    public TransactionType? TransactionType { get; set; }
    public string OrderId { get; set; }
    public string WalletNumber { get; set; }
    public string MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public ChargebackStatus? Status { get; set; }
    public string UserName { get; set; }
}

public class GetChargebackQueryHandler : IRequestHandler<GetChargebackQuery, PaginatedList<ChargebackDto>>
{
    private readonly IChargebackService _chargebackService;

    public GetChargebackQueryHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<PaginatedList<ChargebackDto>> Handle(GetChargebackQuery request, CancellationToken cancellationToken)
    {
        return await _chargebackService.GetChargebackAsync(request);
    }
}
