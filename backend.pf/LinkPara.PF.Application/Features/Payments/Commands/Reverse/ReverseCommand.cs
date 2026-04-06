using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.Reverse;

public class ReverseCommand : IRequest<ReverseResponse>
{
    public Guid MerchantId { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public bool? IsTopUpPayment { get; set; }
}

public class ReverseCommandHandler : IRequestHandler<ReverseCommand, ReverseResponse>
{
    private readonly IReverseService _reverseService;

    public ReverseCommandHandler(IReverseService paymentService)
    {
        _reverseService = paymentService;
    }

    public async Task<ReverseResponse> Handle(ReverseCommand request, CancellationToken cancellationToken)
    {
        return await _reverseService.ReverseAsync(request);
    }
}