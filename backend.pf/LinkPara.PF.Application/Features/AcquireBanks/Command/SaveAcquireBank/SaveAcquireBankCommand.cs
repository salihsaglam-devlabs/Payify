using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;

public class SaveAcquireBankCommand : IRequest
{
    public int BankCode { get; set; }
    public int EndOfDayHour { get; set; }
    public int EndOfDayMinute { get; set; }
    public bool AcceptAmex { get; set; }
    public bool HasSubmerchantIntegration { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public bool RestrictOwnCardNotOnUs { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
}

public class SaveAcquireBankCommandHandler : IRequestHandler<SaveAcquireBankCommand>
{
    private readonly IAcquireBankService _acquireBankService;

    public SaveAcquireBankCommandHandler(IAcquireBankService acquireBankService)
    {
        _acquireBankService = acquireBankService;
    }
    public async Task<Unit> Handle(SaveAcquireBankCommand request, CancellationToken cancellationToken)
    {
        await _acquireBankService.SaveAsync(request);

        return Unit.Value;
    }
}
