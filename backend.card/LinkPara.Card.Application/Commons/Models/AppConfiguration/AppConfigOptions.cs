using LinkPara.Card.Application.Commons.Models.AuthBypass;
using LinkPara.Card.Application.Commons.Models.DatabaseConfiguration;

namespace LinkPara.Card.Application.Commons.Models.AppConfiguration;

public class AppConfigOptions
{
    public const string SectionName = "AppConfig";

    public AuthBypassOptions AuthBypass { get; set; }

    public DatabaseOptions Database { get; set; }

    public void ValidateAndApplyDefaults()
    {
        AuthBypass ??= new AuthBypassOptions();
        Database ??= new DatabaseOptions();

        AuthBypass.ValidateAndApplyDefaults();
        Database.ValidateAndApplyDefaults();
    }
}

