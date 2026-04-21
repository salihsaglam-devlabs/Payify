namespace LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

public sealed class DynamicReportingCatalogRow
{
    public string ReportName { get; set; } = string.Empty;
    public string FullViewName { get; set; } = string.Empty;
    public string RequestContractJson { get; set; } = string.Empty;
    public string ResponseContractJson { get; set; } = string.Empty;
}

