namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ProfileOptions
{
    public string Pattern { get; set; }
    public string DefaultEncoding { get; set; } = "UTF-8";
    public List<string> FileExtensions { get; set; } = new();
    public ParsingOptions Parsing { get; set; } = new();
}
