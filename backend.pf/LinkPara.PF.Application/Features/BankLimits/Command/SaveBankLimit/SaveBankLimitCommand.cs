using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;

public class SaveBankLimitCommand : IRequest
{
    public Guid AcquireBankId { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public DateTime LastValidDate { get; set; }
}

public class SaveBankLimitCommandHandler : IRequestHandler<SaveBankLimitCommand>
{
    private readonly IBankLimitService _bankLimitService;
    public SaveBankLimitCommandHandler(IBankLimitService bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }
    public async Task<Unit> Handle(SaveBankLimitCommand request, CancellationToken cancellationToken)
    {
        await _bankLimitService.SaveAsync(request);

        return Unit.Value;
    }
}
