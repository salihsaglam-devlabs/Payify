using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.BankHealthChecks.Queries.GetAllBankHealthCheck;

public class GetAllBankHealthCheckQuery : SearchQueryParams, IRequest<PaginatedList<BankHealthCheckDto>>
{
    public DateTime? LastCheckDateStart { get; set; }
    public DateTime? LastCheckDateEnd { get; set; }
    public Guid? AcquireBankId { get; set; }
    public HealthCheckType? HealthCheckType { get; set; }
}
public class GetAllBankHealthCheckQueryHandler : IRequestHandler<GetAllBankHealthCheckQuery, PaginatedList<BankHealthCheckDto>>
{
    private readonly IBankHealthCheckService _bankHealthcheckService;
    public GetAllBankHealthCheckQueryHandler(IBankHealthCheckService bankHealthcheckService)
    {
        _bankHealthcheckService = bankHealthcheckService;
    }
    public async Task<PaginatedList<BankHealthCheckDto>> Handle(GetAllBankHealthCheckQuery request, CancellationToken cancellationToken)
    {
        return await _bankHealthcheckService.GetListAsync(request);
    }
}