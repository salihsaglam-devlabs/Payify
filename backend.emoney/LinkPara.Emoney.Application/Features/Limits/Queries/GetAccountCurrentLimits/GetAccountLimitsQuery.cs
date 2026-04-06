using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;

public class GetAccountLimitsQuery : IRequest<AccountLimitDto>
{
    public Guid AccountId { get; set; }
    public string CurrencyCode { get; set; }
}

public class GetAccountLimitsQueryHandler : IRequestHandler<GetAccountLimitsQuery, AccountLimitDto>
{
    private readonly ILimitService _limitService;

    public GetAccountLimitsQueryHandler(ILimitService limitService)
    {
        _limitService = limitService;
    }

    public async Task<AccountLimitDto> Handle(GetAccountLimitsQuery request,
        CancellationToken cancellationToken)
    {
        return await _limitService.GetAccountLimitsQuery(request);
    }
}