using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.BankHealthChecks.Command.UpdateBankHealthCheck;

public class UpdateBankHealthCheckCommand : IRequest
{
    public Guid Id { get; set; }
    public bool IsHealthCheckAllowed { get; set; }
}
public class UpdateBankHealthCheckCommandHandler : IRequestHandler<UpdateBankHealthCheckCommand>
{
    private readonly IBankHealthCheckService _bankHealthcheckService;
    public UpdateBankHealthCheckCommandHandler(IBankHealthCheckService bankHealthcheckService)
    {
        _bankHealthcheckService = bankHealthcheckService;
    }
    public async Task<Unit> Handle(UpdateBankHealthCheckCommand request, CancellationToken cancellationToken)
    {
        await _bankHealthcheckService.UpdateAsync(request);

        return Unit.Value;
    }
}