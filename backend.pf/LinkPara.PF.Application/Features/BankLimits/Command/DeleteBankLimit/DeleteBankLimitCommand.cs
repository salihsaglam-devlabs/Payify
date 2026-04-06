using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.BankLimits.Command.DeleteBankLimit;

public class DeleteBankLimitCommand : IRequest
{
    public Guid Id { get; set; }
}
public class DeleteBankLimitCommandHandler : IRequestHandler<DeleteBankLimitCommand>
{
    private readonly IBankLimitService _bankLimitService;
    public DeleteBankLimitCommandHandler(IBankLimitService bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }
    public async Task<Unit> Handle(DeleteBankLimitCommand request, CancellationToken cancellationToken)
    {
        await _bankLimitService.DeleteAsync(request);

        return Unit.Value;
    }
}
