namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class ParsedFileLine
{
    public string RecordType { get; set; }
    public string RawLine { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public object ParsedDataModel { get; set; }
}
