using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposById;

public class GetVposByIdQuery : IRequest<VposDto>
{
    public Guid Id { get; set; }
}

public class GetVposByIdQueryHandler : IRequestHandler<GetVposByIdQuery, VposDto>
{
    private readonly IVposService _vposService;

    public GetVposByIdQueryHandler(IVposService vposService)
    {
        _vposService = vposService;
    }

    public async Task<VposDto> Handle(GetVposByIdQuery request, CancellationToken cancellationToken)
    {
       return await _vposService.GetByIdAsync(request.Id);
    }
}
