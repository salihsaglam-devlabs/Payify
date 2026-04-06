namespace LinkPara.Emoney.Application.Features.AccountServiceProviders;

public class IdentityInfoDto
{
    /// <summary>
    /// Gets or sets the identity type.
    /// </summary>
    public string KmlkTur { get; set; }

    /// <summary>
    /// Gets or sets the identity version.
    /// </summary>
    public string KmlkVrs { get; set; }

    /// <summary>
    /// Gets or sets the related identity type.
    /// </summary>
    public string KrmKmlkTur { get; set; }

    /// <summary>
    /// Gets or sets the related identity version.
    /// </summary>
    public string KrmKmlkVrs { get; set; }

    /// <summary>
    /// Gets or sets the OHK type.
    /// </summary>
    public string OhkTur { get; set; }

    /// <summary>
    /// Gets or sets the Unv.
    /// </summary>
    public string Unv { get; set; }
}