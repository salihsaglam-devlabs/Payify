using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Commands;

public class InitChargebackCommand : IRequest<ChargebackDto>
{
    public Guid TransactionId { get; set; }    
    public string WalletNumber { get; set; }
    public string Description { get; set; }
    public string MerchantId { get; set; }
}

public class InitChargebackCommandHandler : IRequestHandler<InitChargebackCommand, ChargebackDto>
{
    
    private readonly IChargebackService _chargebackService;


    public InitChargebackCommandHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<ChargebackDto> Handle(InitChargebackCommand request, CancellationToken cancellationToken)
    {
        return await _chargebackService.InitChargebackAsync(request);        
    }
}