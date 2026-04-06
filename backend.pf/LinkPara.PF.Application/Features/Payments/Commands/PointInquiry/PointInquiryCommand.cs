using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.PointInquiry;

public class PointInquiryCommand : IRequest<PointInquiryResponse>
{
    public string CardToken { get; set; }
    public string LanguageCode { get; set; }
    public string Currency { get; set; }
    public string ClientIpAddress { get; set; }
    public Guid MerchantId { get; set; }
}

public class PointInquiryCommandHandler : IRequestHandler<PointInquiryCommand, PointInquiryResponse>
{
    private readonly IPointInquiryService _pointInquiryService;

    public PointInquiryCommandHandler(IPointInquiryService pointInquiryService)
    {
        _pointInquiryService = pointInquiryService;
    }

    public async Task<PointInquiryResponse> Handle(PointInquiryCommand request,
        CancellationToken cancellationToken)
    {
        return await _pointInquiryService.PointInquiryAsync(request);
    }
}
