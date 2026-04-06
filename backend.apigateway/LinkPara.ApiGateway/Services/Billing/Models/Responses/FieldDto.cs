namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class FieldDto
{
    public string Label { get; set; }
    public string Mask { get; set; }
    public string Pattern { get; set; }
    public string Placeholder { get; set; }
    public int Length { get; set; }
    public int Order { get; set; }
    public string Prefix { get; set; }
    public string Suffix { get; set; }
}