using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Commands;

public class ApproveChargebackCommand : IRequest<ChargebackDto>
{
    public Guid TransactionId { get; set; }
    public ChargebackStatus Status { get; set; }   
    public string Description { get; set; }
}

public class ApproveChargebackCommandHandler : IRequestHandler<ApproveChargebackCommand, ChargebackDto>
{
    
    private readonly IChargebackService _chargebackService;


    public ApproveChargebackCommandHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<ChargebackDto> Handle(ApproveChargebackCommand request, CancellationToken cancellationToken)
    {
        return await _chargebackService.ApproveChargebackAsync(request);        
    }
}