using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.Links.Command.SaveLink;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.LinkPayments.Commands.SaveLinkPayment;

public class SaveLinkPaymentCommand : IRequest<LinkPaymentResponse>
{
    public string LinkCode { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public decimal Amount { get; set; }
    public string CardToken { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public string CardHolderName { get; set; }
    public string Gateway { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerNote { get; set; }
}
public class SaveLinkPaymentCommandHandler : IRequestHandler<SaveLinkPaymentCommand, LinkPaymentResponse>
{
    private readonly ILinkPaymentService _linkPaymentService;
    public SaveLinkPaymentCommandHandler(ILinkPaymentService linkPaymentService)
    {
        _linkPaymentService = linkPaymentService;
    }
    public async Task<LinkPaymentResponse> Handle(SaveLinkPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _linkPaymentService.SaveLinkPaymentAsync(request);
    }
}