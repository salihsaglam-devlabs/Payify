using LinkPara.Card.Application.Commons.Extensions;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Flow;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.GetAlerts;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Reviews;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation;

internal sealed class ReconciliationService : IReconciliationService
{
    private readonly IEvaluateService _evaluateService;
    private readonly ExecuteService _executeService;
    private readonly ReviewService _reviewService;
    private readonly GetAlertsService _alertsService;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;

    public ReconciliationService(
        IEvaluateService evaluateService,
        ExecuteService executeService,
        ReviewService reviewService,
        GetAlertsService alertsService,
        IReconciliationErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _evaluateService = evaluateService;
        _executeService = executeService;
        _reviewService = reviewService;
        _alertsService = alertsService;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_EVALUATE"));
            return new EvaluateResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _evaluateService.EvaluateAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "RECONCILIATION_SERVICE_EVALUATE"));
            return new EvaluateResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.EvaluationFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_EXECUTE"));
            return new ExecuteResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _executeService.ExecuteAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "RECONCILIATION_SERVICE_EXECUTE"));
            return new ExecuteResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.OperationExecutionFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<ApproveResponse> ApproveAsync(ApproveRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_APPROVE"));
            return new ApproveResponse
            {
                Result = "Failed",
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _reviewService.ApproveAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(
                ex,
                "RECONCILIATION_SERVICE_APPROVE",
                operationId: request.OperationId));
            return new ApproveResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.ManualReviewApprovalFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<RejectResponse> RejectAsync(RejectRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_REJECT"));
            return new RejectResponse
            {
                Result = "Failed",
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _reviewService.RejectAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(
                ex,
                "RECONCILIATION_SERVICE_REJECT",
                operationId: request.OperationId));
            return new RejectResponse
            {
                OperationId = request.OperationId,
                Result = "Failed",
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.ManualReviewRejectionFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<PendingReviewsResponse> GetPendingReviewsAsync(PendingReviewsRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_GET_PENDING_REVIEWS"));
            return new PendingReviewsResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _reviewService.GetPendingAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "RECONCILIATION_SERVICE_GET_PENDING_REVIEWS"));
            return new PendingReviewsResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.PendingReviewsLoadFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<GetAlertsResponse> GetAlertsAsync(GetAlertsRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ReconciliationErrorDetail>();
        if (request is null)
        {
            errors.Add(_errorMapper.Create("INVALID_REQUEST", _localizer.Get("Reconciliation.RequestIsNull"), "RECONCILIATION_SERVICE_GET_ALERTS"));
            return new GetAlertsResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.RequestFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }

        try
        {
            var response = await _alertsService.GetAsync(request, errors, cancellationToken);
            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "RECONCILIATION_SERVICE_GET_ALERTS"));
            return new GetAlertsResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Reconciliation.AlertsLoadFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    private static string BuildFailureMessage(string fallbackMessage, IReadOnlyCollection<ReconciliationErrorDetail> errors)
    {
        var firstMessage = errors.FirstOrDefault()?.Message;
        if (string.IsNullOrWhiteSpace(firstMessage))
        {
            return fallbackMessage;
        }

        return $"{fallbackMessage} {firstMessage}";
    }
}
