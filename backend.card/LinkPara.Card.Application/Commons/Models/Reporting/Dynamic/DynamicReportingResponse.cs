using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

public sealed class DynamicReportingResponse : ReconciliationResponseBase
{
    public string? ReportName { get; set; }
    
    public DynamicReportContracts? Contracts { get; set; }
    
    public List<Dictionary<string, object?>>? Items { get; set; }
    
    public List<DynamicReportingCatalogItem>? Reports { get; set; }
}

