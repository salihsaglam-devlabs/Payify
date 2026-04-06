using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.HostedPayments.Command.InitHostedPayment;

public class InitHostedPaymentCommand : IRequest<InitHostedPaymentResponse>
{
    public Guid MerchantId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
    public bool CommissionFromCustomer { get; set; }
    public bool Is3dRequired { get; set; }
    public string CallbackUrl { get; set; }
    public string ReturnUrl { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public bool EnableInstallments { get; set; }
    public List<HostedPaymentInstallmentDto> Installments { get; set; }
    public string ConversationId { get; set; }
    public HppPageViewType PageViewType { get; set; }
}

public class InitHostedPaymentCommandHandler : IRequestHandler<InitHostedPaymentCommand,InitHostedPaymentResponse>
{
    private readonly IHostedPaymentService _hostedPaymentService;
    
    public InitHostedPaymentCommandHandler(IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentService = hostedPaymentService;
    }
    public async Task<InitHostedPaymentResponse> Handle(InitHostedPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _hostedPaymentService.InitHostedPaymentAsync(request);
    }
}