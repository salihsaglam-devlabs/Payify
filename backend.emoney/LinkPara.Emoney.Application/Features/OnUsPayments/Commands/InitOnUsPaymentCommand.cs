using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OnUsPayments.Commands;

public class InitOnUsPaymentCommand : IRequest<OnUsPaymentResponse>
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public DateTime ExpireDate { get; set; }
    public DateTime RequestDate { get; set; }
}

public class InitOnUsPaymentCommandHandler : IRequestHandler<InitOnUsPaymentCommand, OnUsPaymentResponse>
{
    
    private readonly IOnUsPaymentService _onUsPaymentService;


    public InitOnUsPaymentCommandHandler(IOnUsPaymentService onUsPaymentService)
    {        
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<OnUsPaymentResponse> Handle(InitOnUsPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.InitOnUsPaymentAsync(request);        
    }
}