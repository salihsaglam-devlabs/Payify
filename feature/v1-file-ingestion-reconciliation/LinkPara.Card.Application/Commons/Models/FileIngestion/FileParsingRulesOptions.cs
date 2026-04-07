using LinkPara.Card.Application.Commons.Constants;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileParsingRulesOptions
{
    public Dictionary<string, FixedWidthFileRule> Files { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class FixedWidthFileRule
{
    public string HeaderPrefix { get; set; } = FixedWidthRecordTypes.Header;
    public string FooterPrefix { get; set; } = FixedWidthRecordTypes.Footer;
    public bool TreatFirstLineAsHeader { get; set; } = true;
    public int DetailLength { get; set; }
    public Dictionary<string, FixedWidthRecordRule> Records { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class FixedWidthRecordRule
{
    public Dictionary<string, FixedWidthFieldRule> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class FixedWidthFieldRule
{
    public int Start { get; set; }
    public int Length { get; set; }
}
