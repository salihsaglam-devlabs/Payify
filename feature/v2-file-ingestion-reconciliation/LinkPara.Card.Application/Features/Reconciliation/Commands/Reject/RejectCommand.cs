using LinkPara.Card.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Reject
{
    public class RejectCommand : IRequest<RejectResponse>
    {
        public RejectRequest Request { get; set; } = new RejectRequest();
    }

    public class RejectCommandHandler : IRequestHandler<RejectCommand, RejectResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public RejectCommandHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<RejectResponse> Handle(RejectCommand request, CancellationToken cancellationToken)
        {
            return await _service.RejectAsync(request.Request, cancellationToken);
        }
    }
}
