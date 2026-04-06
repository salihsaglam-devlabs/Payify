using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Queries;

public class GetChargebackDocumentQuery : IRequest<List<ChargebackDocumentDto>>
{
    public Guid ChargebackId { get; set; }
}

public class GetChargebackDocumentCommandHandler : IRequestHandler<GetChargebackDocumentQuery, List<ChargebackDocumentDto>>
{

    private readonly IChargebackService _chargebackService;

    public GetChargebackDocumentCommandHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<List<ChargebackDocumentDto>> Handle(GetChargebackDocumentQuery request, CancellationToken cancellationToken)
    {
        return await _chargebackService.GetChargebackDocumentsAsync(request);
    }
}