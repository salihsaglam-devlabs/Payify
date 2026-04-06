using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSession;

public class GetThreeDSessionCommand : IRequest<GetThreeDSessionResponse>
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public string Currency { get; set; }
    public VposPaymentType PaymentType { get; set; }
    public int InstallmentCount { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public bool? IsTopUpPayment { get; set; }
    public bool? IsInsurancePayment { get; set; }
}

public class GetThreeDSessionCommandHandler : IRequestHandler<GetThreeDSessionCommand, GetThreeDSessionResponse>
{
    private readonly IThreeDService _threeDService;

    public GetThreeDSessionCommandHandler(IThreeDService threeDService)
    {
        _threeDService = threeDService;
    }

    public async Task<GetThreeDSessionResponse> Handle(GetThreeDSessionCommand request, CancellationToken cancellationToken)
    {
        return await _threeDService.GetThreeDSessionAsync(request);
    }
}
