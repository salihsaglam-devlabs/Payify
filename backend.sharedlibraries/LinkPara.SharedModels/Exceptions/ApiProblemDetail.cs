namespace LinkPara.SharedModels.Exceptions;

public class ApiProblemDetail
{
    public string Code { get; set; }
    public string Status { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Type { get; set; }
    public IDictionary<string, string[]> Errors { get; set; }
}