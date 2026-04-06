using LinkPara.Card.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts
{
    public class GetAlertsQuery : IRequest<GetAlertsResponse>
    {
        public GetAlertsRequest Request { get; set; } = new GetAlertsRequest();
    }

    public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, GetAlertsResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public GetAlertsQueryHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<GetAlertsResponse> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetAlertsAsync(request.Request, cancellationToken);
        }
    }
}
