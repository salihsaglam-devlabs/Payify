namespace LinkPara.Card.Application.Commons.Models.DatabaseConfiguration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public const bool DefaultEnableAutoMigrate = false;

    public bool? EnableAutoMigrate { get; set; }

    public void ValidateAndApplyDefaults()
    {
        EnableAutoMigrate ??= DefaultEnableAutoMigrate;
    }
}

