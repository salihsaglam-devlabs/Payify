using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.PendingReviews
{
    public class GetPendingReviewsQuery : SearchQueryParams, IRequest<GetPendingReviewsResponse>
    {
        public DateOnly? Date { get; set; }
    }

    public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, GetPendingReviewsResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<GetPendingReviewsQueryHandler> _logger;

        public GetPendingReviewsQueryHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            Func<LocalizerResource, IStringLocalizer> localizerFactory,
            ILogger<GetPendingReviewsQueryHandler> logger)
        {
            _service = service;
            _errorMapper = errorMapper;
            _localizer = localizerFactory(LocalizerResource.Messages);
            _logger = logger;
        }

        public async Task<GetPendingReviewsResponse> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.GetPendingReviewsAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.GetPendingReviewsFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_GET_PENDING_REVIEWS");
                return new GetPendingReviewsResponse
                {
                    Message = _localizer.Get("Handler.Reconciliation.GetPendingReviewsFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }
    }
}
