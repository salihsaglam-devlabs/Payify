using LinkPara.Card.Application.Commons.Extensions;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.GetAlerts;

internal sealed class GetAlertsService
{
    private readonly CardDbContext _dbContext;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;

    public GetAlertsService(
        CardDbContext dbContext,
        IReconciliationErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<GetAlertsResponse> GetAsync(
        GetAlertsRequest request,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        errors ??= new List<ReconciliationErrorDetail>();
        try
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 1000);
            var skip = (page - 1) * pageSize;

            var query = _dbContext.ReconciliationAlerts
                .AsNoTracking()
                .AsQueryable();

            if (request.Date.HasValue)
            {
                var start = request.Date.Value.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);

                query = query.Where(x => x.CreateDate >= start && x.CreateDate < end);
            }

            if (request.AlertStatus.HasValue)
            {
                query = query.Where(x => x.AlertStatus == request.AlertStatus.Value);
            }

            query = query.OrderByDescending(x => x.CreateDate);

            var total = await query.CountAsync(cancellationToken);

            var alerts = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new Application.Commons.Models.Reconciliation.Shared.Alert
                {
                    Id = x.Id,
                    FileLineId = x.FileLineId,
                    GroupId = x.GroupId,
                    EvaluationId = x.EvaluationId,
                    OperationId = x.OperationId,
                    Severity = x.Severity,
                    AlertType = x.AlertType,
                    Message = x.Message,
                    CreatedAt = x.CreateDate
                })
                .ToListAsync(cancellationToken);

            return new GetAlertsResponse
            {
                Page = new PagedResult<Application.Commons.Models.Reconciliation.Shared.Alert>
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    Items = alerts
                },
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(
                ex,
                "GET_ALERTS_QUERY",
                message: _localizer.Get("Reconciliation.AlertsLoadFailed")));

            return new GetAlertsResponse
            {
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }
}
