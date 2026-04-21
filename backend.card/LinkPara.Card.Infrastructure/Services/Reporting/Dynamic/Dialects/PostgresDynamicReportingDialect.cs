using LinkPara.Card.Application.Commons.Interfaces.Reporting;

namespace LinkPara.Card.Infrastructure.Services.Reporting.Dynamic.Dialects;

internal sealed class PostgresDynamicReportingDialect : IDynamicReportingDialect
{
    public string QuoteIdent(string ident)
        => "\"" + ident.Replace("\"", "\"\"") + "\"";

    public string BuildLikeClause(string quotedColumn, string parameterName)
        => $"{quotedColumn} ILIKE {parameterName} ESCAPE '/'";

    public string BuildSelect(string fullViewName, string whereClause, int rowLimit)
        => $"SELECT * FROM {fullViewName}{whereClause} LIMIT {rowLimit}";

    public string CatalogSql => @"
        SELECT
            report_name                  AS ""ReportName"",
            full_view_name               AS ""FullViewName"",
            request_contract_json::text  AS ""RequestContractJson"",
            response_contract_json::text AS ""ResponseContractJson""
        FROM reporting.rep_contract_catalog
        WHERE (@reportName IS NULL OR report_name = @reportName)
        ORDER BY report_name;";
}

