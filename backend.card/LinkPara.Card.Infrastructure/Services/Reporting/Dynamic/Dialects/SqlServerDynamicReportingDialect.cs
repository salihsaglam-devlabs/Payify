using LinkPara.Card.Application.Commons.Interfaces.Reporting;

namespace LinkPara.Card.Infrastructure.Services.Reporting.Dynamic.Dialects;

internal sealed class SqlServerDynamicReportingDialect : IDynamicReportingDialect
{
    public string QuoteIdent(string ident)
        => "[" + ident.Replace("]", "]]") + "]";

    public string BuildLikeClause(string quotedColumn, string parameterName)
        => $"{quotedColumn} LIKE {parameterName} ESCAPE '/'";

    public string BuildSelect(string fullViewName, string whereClause, int rowLimit)
        => $"SELECT TOP ({rowLimit}) * FROM {fullViewName}{whereClause}";

    public string CatalogSql => @"
        SELECT
            report_name                                   AS ReportName,
            full_view_name                                AS FullViewName,
            CAST(request_contract_json  AS nvarchar(max)) AS RequestContractJson,
            CAST(response_contract_json AS nvarchar(max)) AS ResponseContractJson
        FROM reporting.rep_contract_catalog
        WHERE (@reportName IS NULL OR report_name = @reportName)
        ORDER BY report_name;";
}

