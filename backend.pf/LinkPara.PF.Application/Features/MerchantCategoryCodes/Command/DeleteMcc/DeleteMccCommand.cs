using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;

public class DeleteMccCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteMccCommandHandler : IRequestHandler<DeleteMccCommand>
{
    private readonly IMccService _mccService;

    public DeleteMccCommandHandler(IMccService mccService)
    {
        _mccService = mccService;
    }

    public async Task<Unit> Handle(DeleteMccCommand request, CancellationToken cancellationToken)
    {
        await _mccService.DeleteAsync(request);

        return Unit.Value;
    }
}
