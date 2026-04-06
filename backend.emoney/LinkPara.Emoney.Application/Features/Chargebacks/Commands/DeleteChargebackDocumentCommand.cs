using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Commands;

public class DeleteChargebackDocumentCommand : IRequest<bool>
{
    public Guid ChargebackDocumentId { get; set; }   
}

public class DeleteChargebackDocumentCommandHandler : IRequestHandler<DeleteChargebackDocumentCommand, bool>
{
    
    private readonly IChargebackService _chargebackService;

    public DeleteChargebackDocumentCommandHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<bool> Handle(DeleteChargebackDocumentCommand request, CancellationToken cancellationToken)
    {
        return await _chargebackService.DeleteChargebackDocumentAsync(request);        
    }
}