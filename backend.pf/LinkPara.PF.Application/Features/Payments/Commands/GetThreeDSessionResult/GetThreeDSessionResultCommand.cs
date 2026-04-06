using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSessionResult;

public class GetThreeDSessionResultCommand : IRequest<GetThreeDSessionResultResponse>
{
    public string ConversationId { get; set; }
    public string ThreeDSessionId { get; set; }
    public Guid MerchantId { get; set; }
    public string LanguageCode { get; set; }
}

public class GetThreeDSessionResultCommandHandler : 
    IRequestHandler<GetThreeDSessionResultCommand, GetThreeDSessionResultResponse>
{
    private readonly IThreeDService _threeDService;

    public GetThreeDSessionResultCommandHandler(IThreeDService threeDService)
    {
        _threeDService = threeDService;
    }

    public async Task<GetThreeDSessionResultResponse> Handle(GetThreeDSessionResultCommand request, CancellationToken cancellationToken)
    {
        return await _threeDService.GetThreeDSessionResultAsync(request);
    }
}

