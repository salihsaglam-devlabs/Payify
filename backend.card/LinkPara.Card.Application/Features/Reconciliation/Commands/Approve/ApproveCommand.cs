using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Approve
{
    public class ApproveCommand : IRequest<ApproveResponse>
    {
        public ApproveRequest Request { get; set; } = new ApproveRequest();
    }

    public class ApproveCommandHandler : IRequestHandler<ApproveCommand, ApproveResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<ApproveCommandHandler> _logger;

        public ApproveCommandHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            Func<LocalizerResource, IStringLocalizer> localizerFactory,
            ILogger<ApproveCommandHandler> logger)
        {
            _service = service;
            _errorMapper = errorMapper;
            _localizer = localizerFactory(LocalizerResource.Messages);
            _logger = logger;
        }

        public async Task<ApproveResponse> Handle(ApproveCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.ApproveAsync(request.Request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.ApproveFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_RECONCILIATION_APPROVE",
                    operationId: request.Request?.OperationId);
                return new ApproveResponse
                {
                    OperationId = request.Request?.OperationId ?? Guid.Empty,
                    Result = "Failed",
                    Message = _localizer.Get("Handler.Reconciliation.ApproveFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }
    }
}
