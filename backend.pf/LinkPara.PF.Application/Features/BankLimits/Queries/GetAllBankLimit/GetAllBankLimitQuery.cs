using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.BankLimits.Queries.GetAllBankLimit;

public class GetAllBankLimitQuery : SearchQueryParams, IRequest<PaginatedList<BankLimitDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public Guid? AcquireBankId { get; set; }
    public decimal? MonthlyLimitAmount { get; set; }
    public BankLimitType? BankLimitType { get; set; }
    public DateTime? LastValidDate { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public bool? IsExpired { get; set; }
}
public class GetAllBankLimitQueryHandler : IRequestHandler<GetAllBankLimitQuery, PaginatedList<BankLimitDto>>
{
    private readonly IBankLimitService _bankLimitService;
    public GetAllBankLimitQueryHandler(IBankLimitService bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }
    public async Task<PaginatedList<BankLimitDto>> Handle(GetAllBankLimitQuery request, CancellationToken cancellationToken)
    {
        return await _bankLimitService.GetListAsync(request);
    }
}
