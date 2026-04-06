using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.Return;
public class ReturnCommand : IRequest<ReturnResponse>
{
    public Guid MerchantId { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public bool IsAdminApproved { get; set; }
    public Guid MerchantReturnPoolId { get; set; }
    public bool? IsTopUpPayment { get; set; }
}

public class ReturnCommandHandler : IRequestHandler<ReturnCommand, ReturnResponse>
{
    private readonly IReturnService _returnService;

    public ReturnCommandHandler(IReturnService returnService)
    {
        _returnService = returnService;
    }

    public async Task<ReturnResponse> Handle(ReturnCommand request, CancellationToken cancellationToken)
    {
        return await _returnService.ReturnAsync(request);
    }
}