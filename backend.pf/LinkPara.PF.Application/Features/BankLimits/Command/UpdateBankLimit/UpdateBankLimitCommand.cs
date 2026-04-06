using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.BankLimits.Command.UpdateBankLimit;

public class UpdateBankLimitCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid AcquireBankId { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public DateTime LastValidDate { get; set; }
}
public class UpdateBankLimitCommandHandler : IRequestHandler<UpdateBankLimitCommand>
{
    private readonly IBankLimitService _bankLimitService;
    public UpdateBankLimitCommandHandler(IBankLimitService bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }
    public async Task<Unit> Handle(UpdateBankLimitCommand request, CancellationToken cancellationToken)
    {
        await _bankLimitService.UpdateAsync(request);

        return Unit.Value;
    }
}
