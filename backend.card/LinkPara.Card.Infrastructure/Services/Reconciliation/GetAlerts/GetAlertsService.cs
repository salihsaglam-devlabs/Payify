using LinkPara.Card.Application.Commons.Helpers.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.GetAlerts;

internal sealed class GetAlertsService
{
    private readonly CardDbContext _dbContext;

    public GetAlertsService(CardDbContext dbContext)
    {
        _dbContext = dbContext;
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
                .Select(x => new Alert
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
                Page = new PagedResult<Alert>
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
            errors.Add(ReconciliationErrorMapper.MapException(
                ex,
                "GET_ALERTS_QUERY",
                message: "Failed to load reconciliation alerts."));

            return new GetAlertsResponse
            {
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }
}