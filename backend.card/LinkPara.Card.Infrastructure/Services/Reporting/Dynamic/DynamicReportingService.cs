using System.Data;
using System.Text.Json;
using Dapper;
using LinkPara.Card.Application.Commons.Helpers.Reporting;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;
using LinkPara.Card.Application.Features.Reporting.Queries.Dynamic;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reporting.Dynamic;

internal sealed class DynamicReportingService : IDynamicReportingService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly CardDbContext _db;
    private readonly IDynamicReportingDialect _dialect;
    private readonly DynamicReportingSqlBuilder _builder;

    public DynamicReportingService(
        CardDbContext db,
        IDynamicReportingDialect dialect,
        DynamicReportingSqlBuilder builder)
    {
        _db = db;
        _dialect = dialect;
        _builder = builder;
    }

    public async Task<DynamicReportingResponse> ExecuteAsync(GetDynamicReportingQuery query, CancellationToken ct)
    {
        var catalog = await GetCatalogAsync(query.ReportName, ct);
        
        if (string.IsNullOrWhiteSpace(query.ReportName))
        {
            return new DynamicReportingResponse
            {
                Reports = catalog
                    .Select(c => new DynamicReportingCatalogItem
                    {
                        ReportName = c.ReportName,
                        Contracts  = ParseContracts(c)
                    })
                    .ToList()
            };
        }
        
        var entry = catalog.FirstOrDefault(c => c.ReportName == query.ReportName);
        if (entry is null)
            return Fail(query.ReportName, $"Unknown reportName '{query.ReportName}'");

        var contracts = ParseContracts(entry);
        
        var filterErrors = DynamicReportingValidator.Validate(contracts.Request!, query.Filters);
        if (filterErrors.Count > 0)
            return Fail(query.ReportName, "Invalid filters", filterErrors);

        var response = new DynamicReportingResponse
        {
            ReportName = query.ReportName,
            Contracts  = query.IncludeContract ? contracts : null
        };
        
        if (query.IncludeData)
        {
            var (sql, parameters) = _builder.Build(entry.FullViewName, query.Filters, contracts.Request!);
            response.Items = await ExecuteDataAsync(sql, parameters, ct);
        }

        return response;
    }

    private async Task<IReadOnlyList<DynamicReportingCatalogRow>> GetCatalogAsync(
        string? reportName, CancellationToken ct)
    {
        var conn = await EnsureOpenAsync(ct);
        var rows = await conn.QueryAsync<DynamicReportingCatalogRow>(
            new CommandDefinition(_dialect.CatalogSql, new { reportName }, cancellationToken: ct));
        return rows.AsList();
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteDataAsync(
        string sql,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        var conn = await EnsureOpenAsync(ct);

        var dyn = new DynamicParameters();
        foreach (var kv in parameters)
            dyn.Add(kv.Key, kv.Value);

        var rows = await conn.QueryAsync(new CommandDefinition(sql, dyn, cancellationToken: ct));

        var result = new List<Dictionary<string, object?>>();
        foreach (var row in rows)
        {
            var dict = (IDictionary<string, object?>)row!;
            result.Add(new Dictionary<string, object?>(dict, StringComparer.Ordinal));
        }
        return result;
    }

    private async Task<IDbConnection> EnsureOpenAsync(CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _db.Database.OpenConnectionAsync(ct);
        return conn;
    }

    private static DynamicReportContracts ParseContracts(DynamicReportingCatalogRow c) => new()
    {
        Request  = JsonSerializer.Deserialize<DynamicReportRequestContract>(c.RequestContractJson, JsonOpts) ?? new(),
        Response = JsonSerializer.Deserialize<DynamicReportResponseContract>(c.ResponseContractJson, JsonOpts) ?? new()
    };

    private static DynamicReportingResponse Fail(
        string? reportName, string message, IReadOnlyList<string>? details = null)
    {
        var errs = (details is { Count: > 0 } ? details : new[] { message })
            .Select(d => new ReconciliationErrorDetail
            {
                Code = "DYNAMIC_REPORTING_VALIDATION",
                Message = d,
                Step = "DYNAMIC_REPORTING",
                Severity = "Error"
            })
            .ToList();

        return new DynamicReportingResponse
        {
            ReportName = reportName,
            Message = message,
            ErrorCount = errs.Count,
            Errors = errs
        };
    }
}
