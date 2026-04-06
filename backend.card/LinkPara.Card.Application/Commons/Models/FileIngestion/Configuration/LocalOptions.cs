namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class LocalOptions
{
    public Dictionary<string, PathOptions> Paths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
