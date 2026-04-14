using LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;
using AlertModel = LinkPara.Card.Application.Commons.Models.Reconciliation.Shared.Alert;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.GetAlerts;

internal sealed class GetAlertsService
{
    private readonly CardDbContext _dbContext;

    public GetAlertsService(CardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<AlertModel>> GetAsync(
        GetAlertsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.Size, 1, 1000);
        var skip = (page - 1) * pageSize;

        var baseQuery = _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .AsQueryable();

        if (query.Date.HasValue)
        {
            var start = query.Date.Value.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            baseQuery = baseQuery.Where(x => x.CreateDate >= start && x.CreateDate < end);
        }

        if (query.AlertStatus.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.AlertStatus == query.AlertStatus.Value);
        }

        baseQuery = baseQuery.OrderByDescending(x => x.CreateDate);

        var total = await baseQuery.CountAsync(cancellationToken);

        var alerts = await baseQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new AlertModel
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

        return new PaginatedList<AlertModel>(alerts, total, page, pageSize, OrderByStatus.Desc, query.SortBy);
    }
}
