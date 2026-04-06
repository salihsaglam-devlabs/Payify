using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.HostedPayments.Command.TriggerHppWebhook;

public class TriggerHppWebhookCommand : IRequest
{
    public string TrackingId { get; set; }
}

public class TriggerHppWebhookCommandHandler : IRequestHandler<TriggerHppWebhookCommand>
{

    private readonly IHostedPaymentService _hostedPaymentService;
    
    public TriggerHppWebhookCommandHandler(IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentService = hostedPaymentService;
    }

    public async Task<Unit> Handle(TriggerHppWebhookCommand request, CancellationToken cancellationToken)
    {
        await _hostedPaymentService.TriggerHppWebhookAsync(request.TrackingId);
        return Unit.Value;  
    }
}