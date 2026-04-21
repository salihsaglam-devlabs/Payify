namespace LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

public sealed class DynamicReportingFilter
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object? Value { get; set; }
}

