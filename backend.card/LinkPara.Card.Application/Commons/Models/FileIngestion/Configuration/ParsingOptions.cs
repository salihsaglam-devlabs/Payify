namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ParsingOptions
{
    public int DetailLength { get; set; }
    public string HeaderPrefix { get; set; }
    public string FooterPrefix { get; set; }
    public bool TreatFirstLineAsHeader { get; set; }
    public Dictionary<string, RecordOptions> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
