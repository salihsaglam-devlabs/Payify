using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;

public class DeleteAcquireBankCommand : IRequest
{
    public Guid Id { get; set; }
}

 public class DeleteAcquireBankCommandHandler : IRequestHandler<DeleteAcquireBankCommand>
{
    private readonly IAcquireBankService _acquireBankService;

    public DeleteAcquireBankCommandHandler(IAcquireBankService acquireBankService)
    {
        _acquireBankService = acquireBankService;
    }

    public async Task<Unit> Handle(DeleteAcquireBankCommand request, CancellationToken cancellationToken)
    {
        await _acquireBankService.DeleteAsync(request);

        return Unit.Value;  
    }
}
