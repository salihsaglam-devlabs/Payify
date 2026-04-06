using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccById;

public class GetMccByIdQuery : IRequest<MccDto>
{
    public Guid Id { get; set; }
}

public class GetMccByIdQueryHandler : IRequestHandler<GetMccByIdQuery, MccDto>
{
    private readonly IMccService _mccService;

    public GetMccByIdQueryHandler(IMccService mccService)
    {
        _mccService = mccService;
    }
    public async Task<MccDto> Handle(GetMccByIdQuery request, CancellationToken cancellationToken)
    {
        return await _mccService.GetByIdAsync(request.Id);
    }
}
