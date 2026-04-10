using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Approve
{
    public class ApproveCommand : IRequest<ApproveResponse>
    {
        public ApproveRequest Request { get; set; } = new ApproveRequest();
    }

    public class ApproveCommandHandler : IRequestHandler<ApproveCommand, ApproveResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public ApproveCommandHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<ApproveResponse> Handle(ApproveCommand request, CancellationToken cancellationToken)
        {
            return await _service.ApproveAsync(request.Request, cancellationToken);
        }
    }
}
