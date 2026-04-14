using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class FileIngestionOptions
{
    public const string SectionName = "FileIngestion";

    public ProcessingOptions Processing { get; set; }
    public ConnectionsOptions Connections { get; set; }
    public Dictionary<string, ProfileOptions> Profiles { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Processing ??= new ProcessingOptions();
        Processing.ValidateAndApplyDefaults();

        if (Connections is null)
            throw new FileIngestionConfigConnectionsMissingException("Vault configuration missing: FileIngestion.Connections");
        if (Profiles is null)
            throw new FileIngestionConfigProfilesMissingException("Vault configuration missing: FileIngestion.Profiles");

        Connections.Validate();
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
