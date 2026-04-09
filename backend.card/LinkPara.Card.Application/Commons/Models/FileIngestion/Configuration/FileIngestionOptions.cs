namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionOptions
{
    public const string SectionName = "FileIngestion";

    public ProcessingOptions Processing { get; set; }
    public ConnectionsOptions Connections { get; set; }
    public Dictionary<string, ProfileOptions> Profiles { get; set; }

    public void Validate()
    {
        if (Processing is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Processing");
        if (Connections is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Connections");
        if (Profiles is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Profiles");

        Processing.Validate();
        Connections.Validate();
    }
}
