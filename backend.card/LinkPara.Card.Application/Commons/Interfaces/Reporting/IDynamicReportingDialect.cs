namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IDynamicReportingDialect
{
    string QuoteIdent(string ident);
    
    string BuildLikeClause(string quotedColumn, string parameterName);
    
    string BuildSelect(string fullViewName, string whereClause, int rowLimit);
    
    string CatalogSql { get; }
}

