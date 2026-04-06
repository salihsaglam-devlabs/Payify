using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.Provision;

public class ProvisionCommand : IRequest<ProvisionResponse>, IClientApiCommand
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public VposPaymentType PaymentType { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string ConversationId { get; set; }
    public string OriginalOrderId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public string MerchantCustomerName { get; set; }
    public string CardHolderName { get; set; }
    public string Description { get; set; }
    public string Gateway { get; set; }
    public string MerchantCustomerPhoneCode { get; set; }
    public string MerchantCustomerPhoneNumber { get; set; }
    public bool? IsOnUsPayment { get; set; }
    public bool? IsTopUpPayment { get; set; }
    public string CallbackUrl { get; set; }
    public bool? IsInsurancePayment { get; set; }
    public string CardHolderIdentityNumber { get; set; }
}

public class ProvisionCommandHandler : IRequestHandler<ProvisionCommand, ProvisionResponse>
{
private readonly IPaymentService _paymentService;

    public ProvisionCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<ProvisionResponse> Handle(ProvisionCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.ProvisionAsync(request);
    }
}
