using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.VerifyOnUsPayment;
public class VerifyOnUsPaymentCommand : IRequest<ProvisionResponse>
{
    public bool IsVerifiedByUser { get; set; }
    public string OrderId { get; set; }
    public string ConversationId { get; set; }
    public string MerchantNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public string LanguageCode { get; set; }
    public string ClientIpAddress { get; set; }
}

public class VerifyOnUsPaymentCommandHandler : IRequestHandler<VerifyOnUsPaymentCommand, ProvisionResponse>
{
    private readonly IOnUsPaymentService _onUsPaymentService;

    public VerifyOnUsPaymentCommandHandler(IOnUsPaymentService onUsPaymentService)
    {
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<ProvisionResponse> Handle(VerifyOnUsPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.CompleteOnUsProvisionAsync(request);
    }
}