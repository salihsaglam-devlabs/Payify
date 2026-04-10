using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate
{
    public class EvaluateCommand : IRequest<EvaluateResponse>
    {
        public EvaluateRequest Request { get; set; } = new EvaluateRequest();
    }

    public class EvaluateCommandHandler : IRequestHandler<EvaluateCommand, EvaluateResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public EvaluateCommandHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<EvaluateResponse> Handle(EvaluateCommand request, CancellationToken cancellationToken)
        {
            return await _service.EvaluateAsync(request.Request, cancellationToken);
        }
    }
}
