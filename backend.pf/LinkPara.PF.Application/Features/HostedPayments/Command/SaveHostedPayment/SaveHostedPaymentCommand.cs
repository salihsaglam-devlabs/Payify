using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.HostedPayments.Command.SaveHostedPayment;

public class SaveHostedPaymentCommand : IRequest<HostedPaymentResponse>
{
    public string ConversationId { get; set; }
    public Guid MerchantId { get; set; }
    public string ClientIpAddress { get; set; }
    public string Gateway { get; set; }
    public string TrackingId { get; set; }
    public decimal Amount { get; set; }
    public string CardHolderName { get; set; }
    public string CardToken { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string LanguageCode { get; set; }
}

public class SaveHostedPaymentCommandHandler : IRequestHandler<SaveHostedPaymentCommand, HostedPaymentResponse>
{
    private readonly IHostedPaymentService _hostedPaymentService;
    
    public SaveHostedPaymentCommandHandler(IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentService = hostedPaymentService;
    }
    
    public async Task<HostedPaymentResponse> Handle(SaveHostedPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _hostedPaymentService.SaveHostedPaymentAsync(request);
    }
}