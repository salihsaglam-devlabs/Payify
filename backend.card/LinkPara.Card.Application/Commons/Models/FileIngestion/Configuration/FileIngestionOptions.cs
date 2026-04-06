namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionOptions
{
    public const string SectionName = "FileIngestion";

    public ProcessingOptions Processing { get; set; } = new();
    public ConnectionsOptions Connections { get; set; } = new();
    public Dictionary<string, ProfileOptions> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
