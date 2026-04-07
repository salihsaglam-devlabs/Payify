namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ParsedFileRecord
{
    public int LineNo { get; set; }
    public string RecordType { get; set; }
    public string RawLine { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
