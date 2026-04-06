using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.Inquire;

public class InquireCommand : IRequest<InquireResponse>
{
    public string PaymentConversationId { get; set; }
    public string OrderId { get; set; }
    public Guid MerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
}

public class InquireCommandHandler : IRequestHandler<InquireCommand, InquireResponse>
{
    private readonly IInquireService _inquireService;

    public InquireCommandHandler(IInquireService inquireService)
    {
        _inquireService = inquireService;
    }

    public async Task<InquireResponse> Handle(InquireCommand request, CancellationToken cancellationToken)
    {
        return await _inquireService.InquireAsync(request);
    }
}