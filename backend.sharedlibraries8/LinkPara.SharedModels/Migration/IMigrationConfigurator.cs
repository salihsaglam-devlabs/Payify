namespace LinkPara.SharedModels.Migration;

public interface IMigrationConfigurator
{
    void Migrate(string connectionString, string databaseProvider = "nondivided");
}