using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Execute
{
    public class ExecuteCommand : IRequest<ExecuteResponse>
    {
        public ExecuteRequest Request { get; set; } = new ExecuteRequest();
    }

    public class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, ExecuteResponse>
    {
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;

        public ExecuteCommandHandler(LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service)
        {
            _service = service;
        }

        public async Task<ExecuteResponse> Handle(ExecuteCommand request, CancellationToken cancellationToken)
        {
            return await _service.ExecuteAsync(request.Request, cancellationToken);
        }
    }
}
