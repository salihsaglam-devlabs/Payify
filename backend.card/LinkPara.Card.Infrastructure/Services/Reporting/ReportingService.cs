using LinkPara.Card.Application.Commons.Helpers.Reporting;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.Card.Application.Features.Reporting.Queries.GetProblemRecords;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByFile;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByNetwork;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryDaily;
using LinkPara.Card.Application.Features.Reporting.Queries.GetTransactions;
using LinkPara.Card.Application.Features.Reporting.Queries.GetUnmatchedRecords;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reporting;

internal sealed class ReportingService : IReportingService
{
    private readonly CardDbContext _dbContext;

    public ReportingService(CardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetTransactionsResponse> GetTransactionsAsync(
        GetTransactionsQuery query, CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Set<ReconciliationTransactionDto>()
            .AsNoTracking()
            .AsQueryable();

        if (query.DateFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate <= query.DateTo.Value);

        var networkDb = ReportingEnumMapper.ToDbValue(query.Network);
        if (networkDb != null)
            baseQuery = baseQuery.Where(x => x.Network == networkDb);

        var matchStatusDb = ReportingEnumMapper.ToDbValue(query.MatchStatus);
        if (matchStatusDb != null)
            baseQuery = baseQuery.Where(x => x.MatchStatus == matchStatusDb);

        if (query.HasAmountMismatch.HasValue)
            baseQuery = baseQuery.Where(x => x.HasAmountMismatch == query.HasAmountMismatch.Value);

        if (query.HasCurrencyMismatch.HasValue)
            baseQuery = baseQuery.Where(x => x.HasCurrencyMismatch == query.HasCurrencyMismatch.Value);

        if (query.HasDateMismatch.HasValue)
            baseQuery = baseQuery.Where(x => x.HasDateMismatch == query.HasDateMismatch.Value);

        if (query.HasStatusMismatch.HasValue)
            baseQuery = baseQuery.Where(x => x.HasStatusMismatch == query.HasStatusMismatch.Value);

        var duplicateStatusDb = ReportingEnumMapper.ToDbValue(query.DuplicateStatus);
        if (duplicateStatusDb != null)
            baseQuery = baseQuery.Where(x => x.DuplicateStatus == duplicateStatusDb);

        var isDescending = query.OrderBy == OrderByStatus.Desc;
        var sortByEnum = ParseSortBy(query.SortBy);
        baseQuery = ApplySorting(baseQuery, sortByEnum, isDescending);

        return await PaginateTransactionsAsync(baseQuery, query, ct);
    }

    public async Task<GetTransactionsResponse> GetProblemRecordsAsync(
        GetProblemRecordsQuery query, CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Set<ReconciliationTransactionDto>()
            .FromSqlRaw("SELECT * FROM reporting.vw_reconciliation_problem_records")
            .AsNoTracking();

        var networkDb = ReportingEnumMapper.ToDbValue(query.Network);
        if (networkDb != null)
            baseQuery = baseQuery.Where(x => x.Network == networkDb);

        if (query.DateFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate <= query.DateTo.Value);

        var isDescending = query.OrderBy == OrderByStatus.Desc;
        var sortByEnum = ParseSortBy(query.SortBy);
        baseQuery = ApplySorting(baseQuery, sortByEnum, isDescending);

        return await PaginateTransactionsAsync(baseQuery, query, ct);
    }

    public async Task<GetTransactionsResponse> GetUnmatchedRecordsAsync(
        GetUnmatchedRecordsQuery query, CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Set<ReconciliationTransactionDto>()
            .FromSqlRaw("SELECT * FROM reporting.vw_reconciliation_unmatched_card")
            .AsNoTracking();

        var networkDb = ReportingEnumMapper.ToDbValue(query.Network);
        if (networkDb != null)
            baseQuery = baseQuery.Where(x => x.Network == networkDb);

        if (query.DateFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            baseQuery = baseQuery.Where(x => x.CardTransactionDate <= query.DateTo.Value);

        var isDescending = query.OrderBy == OrderByStatus.Desc;
        var sortByEnum = ParseSortBy(query.SortBy);
        baseQuery = ApplySorting(baseQuery, sortByEnum, isDescending);

        return await PaginateTransactionsAsync(baseQuery, query, ct);
    }

    public async Task<GetSummaryDailyResponse> GetSummaryDailyAsync(
        GetSummaryDailyQuery query, CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Set<ReconciliationSummaryDailyDto>()
            .AsNoTracking()
            .AsQueryable();

        if (query.DateFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.TransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            baseQuery = baseQuery.Where(x => x.TransactionDate <= query.DateTo.Value);

        var data = await baseQuery
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync(ct);

        return new GetSummaryDailyResponse { Data = data };
    }

    public async Task<GetSummaryByNetworkResponse> GetSummaryByNetworkAsync(
        GetSummaryByNetworkQuery query, CancellationToken ct = default)
    {
        var data = await _dbContext.Set<ReconciliationSummaryByNetworkDto>()
            .AsNoTracking()
            .OrderBy(x => x.Network)
            .ToListAsync(ct);

        return new GetSummaryByNetworkResponse { Data = data };
    }

    public async Task<GetSummaryByFileResponse> GetSummaryByFileAsync(
        GetSummaryByFileQuery query, CancellationToken ct = default)
    {
        var data = await _dbContext.Set<ReconciliationSummaryByFileDto>()
            .AsNoTracking()
            .OrderByDescending(x => x.ProblemCount)
            .ToListAsync(ct);

        return new GetSummaryByFileResponse { Data = data };
    }

    public async Task<GetSummaryOverallResponse> GetSummaryOverallAsync(
        CancellationToken ct = default)
    {
        var data = await _dbContext.Set<ReconciliationSummaryOverallDto>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return new GetSummaryOverallResponse { Data = data };
    }

    private static async Task<GetTransactionsResponse> PaginateTransactionsAsync(
        IQueryable<ReconciliationTransactionDto> baseQuery,
        SearchQueryParams query,
        CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.Size, 1, 1000);
        var skip = (page - 1) * pageSize;

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return new GetTransactionsResponse
        {
            Data = new PaginatedList<ReconciliationTransactionDto>(
                items, total, page, pageSize, query.OrderBy, query.SortBy)
        };
    }
    
    private static ReportingSortBy? ParseSortBy(string sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return null;

        return Enum.TryParse<ReportingSortBy>(sortBy.Trim(), ignoreCase: true, out var result)
            ? result
            : null;
    }
    
    private static IQueryable<ReconciliationTransactionDto> ApplySorting(
        IQueryable<ReconciliationTransactionDto> query,
        ReportingSortBy? sortBy,
        bool descending)
    {
        return sortBy switch
        {
            ReportingSortBy.CardTransactionDate => descending
                ? query.OrderByDescending(x => x.CardTransactionDate)
                       .ThenByDescending(x => x.CardCreateDate)
                : query.OrderBy(x => x.CardTransactionDate)
                       .ThenBy(x => x.CardCreateDate),

            ReportingSortBy.CardCreateDate => descending
                ? query.OrderByDescending(x => x.CardCreateDate)
                : query.OrderBy(x => x.CardCreateDate),

            ReportingSortBy.Network => descending
                ? query.OrderByDescending(x => x.Network)
                       .ThenByDescending(x => x.CardTransactionDate)
                : query.OrderBy(x => x.Network)
                       .ThenBy(x => x.CardTransactionDate),

            ReportingSortBy.MatchStatus => descending
                ? query.OrderByDescending(x => x.MatchStatus)
                       .ThenByDescending(x => x.CardTransactionDate)
                : query.OrderBy(x => x.MatchStatus)
                       .ThenBy(x => x.CardTransactionDate),

            ReportingSortBy.AmountDifference => descending
                ? query.OrderByDescending(x => x.AmountDifference)
                       .ThenByDescending(x => x.CardTransactionDate)
                : query.OrderBy(x => x.AmountDifference)
                       .ThenBy(x => x.CardTransactionDate),
            
            _ => query.OrderByDescending(x => x.CardTransactionDate)
                      .ThenByDescending(x => x.CardCreateDate)
        };
    }
}

