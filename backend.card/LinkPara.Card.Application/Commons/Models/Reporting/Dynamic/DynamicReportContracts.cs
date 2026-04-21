namespace LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

public sealed class DynamicReportFilterDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Nullable { get; set; }
    public List<string> Operators { get; set; } = new();
}

public sealed class DynamicReportColumnDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Nullable { get; set; }
    public int Ordinal { get; set; }
}

public sealed class DynamicReportRequestContract
{
    public List<DynamicReportFilterDescriptor> Filters { get; set; } = new();
}

public sealed class DynamicReportResponseContract
{
    public string Type { get; set; } = "Dictionary<string, object?>";
    public List<DynamicReportColumnDescriptor> Columns { get; set; } = new();
}

public sealed class DynamicReportContracts
{
    public DynamicReportRequestContract? Request { get; set; }
    public DynamicReportResponseContract? Response { get; set; }
}

public sealed class DynamicReportingCatalogItem
{
    public string ReportName { get; set; } = string.Empty;
    public DynamicReportContracts Contracts { get; set; } = new();
}

