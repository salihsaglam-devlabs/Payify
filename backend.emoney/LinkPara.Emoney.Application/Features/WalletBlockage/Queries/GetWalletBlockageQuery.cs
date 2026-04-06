using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Queries;

public class GetWalletBlockageQuery : SearchQueryParams, IRequest<PaginatedList<WalletBlockageDto>>
{
    public Guid? WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string AccountName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public WalletBlockageStatus? BlockageStatus { get; set; }
    public string BlockageAdderUserName { get; set; }
}

public class GetWalletBlockageQueryHandler : IRequestHandler<GetWalletBlockageQuery, PaginatedList<WalletBlockageDto>>
{
    private readonly IWalletBlockageService _WalletBlockageService;

    public GetWalletBlockageQueryHandler(IWalletBlockageService WalletBlockageService)
    {
        _WalletBlockageService = WalletBlockageService;
    }

    public async Task<PaginatedList<WalletBlockageDto>> Handle(GetWalletBlockageQuery request, CancellationToken cancellationToken)
    {
        return await _WalletBlockageService.GetWalletBlockageAsync(request);
    }
}
