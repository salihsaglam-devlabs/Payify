using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Reject
{
    public class RejectCommand : IRequest<RejectResponse>
    {
        public RejectRequest Request { get; set; } = new RejectRequest();
    }

    public class RejectCommandHandler : IRequestHandler<RejectCommand, RejectResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<RejectCommandHandler> _logger;

        public RejectCommandHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            Func<LocalizerResource, IStringLocalizer> localizerFactory,
            ILogger<RejectCommandHandler> logger)
        {
            _service = service;
            _errorMapper = errorMapper;
            _localizer = localizerFactory(LocalizerResource.Messages);
            _logger = logger;
        }

        public async Task<RejectResponse> Handle(RejectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.RejectAsync(request.Request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.RejectFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_RECONCILIATION_REJECT",
                    operationId: request.Request?.OperationId);
                return new RejectResponse
                {
                    OperationId = request.Request?.OperationId ?? Guid.Empty,
                    Result = "Failed",
                    Message = _localizer.Get("Handler.Reconciliation.RejectFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }
    }
}
