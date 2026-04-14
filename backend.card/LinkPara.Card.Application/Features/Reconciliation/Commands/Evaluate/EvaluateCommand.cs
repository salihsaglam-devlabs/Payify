using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate
{
    public class EvaluateCommand : IRequest<EvaluateResponse>
    {
        public EvaluateRequest Request { get; set; } = new EvaluateRequest();
    }

    public class EvaluateCommandHandler : IRequestHandler<EvaluateCommand, EvaluateResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<EvaluateCommandHandler> _logger;

        public EvaluateCommandHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            Func<LocalizerResource, IStringLocalizer> localizerFactory,
            ILogger<EvaluateCommandHandler> logger)
        {
            _service = service;
            _errorMapper = errorMapper;
            _localizer = localizerFactory(LocalizerResource.Messages);
            _logger = logger;
        }

        public async Task<EvaluateResponse> Handle(EvaluateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.EvaluateAsync(request.Request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.EvaluateFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_RECONCILIATION_EVALUATE");
                return new EvaluateResponse
                {
                    Message = _localizer.Get("Handler.Reconciliation.EvaluateFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }
    }
}
