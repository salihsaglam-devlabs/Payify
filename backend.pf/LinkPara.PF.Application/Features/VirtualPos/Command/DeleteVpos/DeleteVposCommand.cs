using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;

public class DeleteVposCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteVposCommandHandler : IRequestHandler<DeleteVposCommand>
{
    private readonly IVposService _vposService;

    public DeleteVposCommandHandler(IVposService vposService)
    {
        _vposService = vposService;
    }

    public async Task<Unit> Handle(DeleteVposCommand request, CancellationToken cancellationToken)
    {
        await _vposService.DeleteAsync(request);

        return Unit.Value;
    }
}
