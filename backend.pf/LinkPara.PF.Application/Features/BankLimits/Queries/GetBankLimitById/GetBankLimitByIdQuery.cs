using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.BankLimits.Queries.GetBankLimitById;

public class GetBankLimitByIdQuery : IRequest<BankLimitDto>
{
    public Guid Id { get; set; }
}
public class GetBankLimitByIdQueryHandler : IRequestHandler<GetBankLimitByIdQuery, BankLimitDto>
{
    private readonly IBankLimitService _bankLimitService;

    public GetBankLimitByIdQueryHandler(IBankLimitService bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }

    public async Task<BankLimitDto> Handle(GetBankLimitByIdQuery request, CancellationToken cancellationToken)
    {
        return await _bankLimitService.GetByIdAsync(request.Id);
    }
}
