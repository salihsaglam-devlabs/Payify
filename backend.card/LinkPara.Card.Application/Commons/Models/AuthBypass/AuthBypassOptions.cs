namespace LinkPara.Card.Application.Commons.Models.AuthBypass;

public class AuthBypassOptions
{
    public const string SectionName = "AuthBypass";

    public const bool DefaultEnabled = false;

    public bool? Enabled { get; set; }

    public string[] Controllers { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Enabled ??= DefaultEnabled;
        Controllers ??= Array.Empty<string>();
    }
}

