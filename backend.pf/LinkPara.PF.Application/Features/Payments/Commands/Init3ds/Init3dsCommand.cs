using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.Init3ds;

public class Init3dsCommand : IRequest<Init3dsResponse>
{
    public string ThreeDSessionId { get; set; }
    public string CallbackUrl { get; set; }
    public string LanguageCode { get; set; }
    public Guid MerchantId { get; set; }
    public string ConversationId { get; set; }
    public string CardHolderName { get; set; }
    public string ClientIpAddress { get; set; }
    public bool? IsTopUpPayment { get; set; }
    public bool? IsInsurancePayment { get; set; }
}

public class Init3dsCommandHandler : IRequestHandler<Init3dsCommand, Init3dsResponse>
{
    private readonly IThreeDService _threeDService;

    public Init3dsCommandHandler(IThreeDService threeDService)
    {
        _threeDService = threeDService;
    }

    public async Task<Init3dsResponse> Handle(Init3dsCommand request, CancellationToken cancellationToken)
    {
        return await _threeDService.Init3ds(request);
    }
}