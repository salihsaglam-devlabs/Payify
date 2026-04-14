namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class ProfileOptions
{
    public string Pattern { get; set; }
    public string DefaultEncoding { get; set; }
    public List<string> FileExtensions { get; set; }
    public ParsingOptions Parsing { get; set; }
    public string SourceDateSubfolderFormat { get; set; }
}
