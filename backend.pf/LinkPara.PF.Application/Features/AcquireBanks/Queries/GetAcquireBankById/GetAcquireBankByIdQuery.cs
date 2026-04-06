using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAcquireBankById;

public class GetAcquireBankByIdQuery : IRequest<AcquireBankDto>
{
    public Guid Id { get; set; }
}

public class GetAcquireBankByIdQueryHandler : IRequestHandler<GetAcquireBankByIdQuery, AcquireBankDto>
{
    private readonly IAcquireBankService _acquireBankService;

    public GetAcquireBankByIdQueryHandler(IAcquireBankService acquireBankService)
    {
        _acquireBankService = acquireBankService;
    }

    public async Task<AcquireBankDto> Handle(GetAcquireBankByIdQuery request, CancellationToken cancellationToken)
    {
        return await _acquireBankService.GetByIdAsync(request.Id);
    }
}

