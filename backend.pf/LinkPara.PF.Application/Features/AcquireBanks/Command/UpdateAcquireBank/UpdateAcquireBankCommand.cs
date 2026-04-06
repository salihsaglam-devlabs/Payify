using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;

public class UpdateAcquireBankCommand : IRequest
{
    public Guid Id { get; set; }
    public int BankCode { get; set; }
    public int EndOfDayHour { get; set; }
    public int EndOfDayMinute { get; set; }
    public bool AcceptAmex { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public bool HasSubmerchantIntegration { get; set; }
    public bool RestrictOwnCardNotOnUs { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
}

public class UpdateAcquireBankCommandHandler : IRequestHandler<UpdateAcquireBankCommand>
{
    private readonly IAcquireBankService _acquireBankService;

    public UpdateAcquireBankCommandHandler(IAcquireBankService acquireBankService)
    {
        _acquireBankService = acquireBankService;
    }

    public async Task<Unit> Handle(UpdateAcquireBankCommand request, CancellationToken cancellationToken)
    {
        await _acquireBankService.UpdateAsync(request);

        return Unit.Value;
    }
}
