using EvolveDb;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data.Common;

namespace LinkPara.SharedModels.Migration;

public class MigrationConfigurator : IMigrationConfigurator
{
    private readonly ILogger<MigrationConfigurator> _logger;
    
    public MigrationConfigurator(ILogger<MigrationConfigurator> logger)
    {
        _logger = logger;
    }
    
    public void Migrate(string connectionString, string databaseProvider = "nondivided")
    {
        var sqlPath = databaseProvider switch
        {
            "nondivided" => "Persistence/PostgreSql",
            "MsSql" => "Persistence/SqlMigrations/MsSql",
            "PostgreSql" => "Persistence/SqlMigrations/PostgreSql",
            _ => "Persistence/PostgreSql"
        };

        DbConnection connection = databaseProvider switch
        {
            "nondivided" => new NpgsqlConnection(connectionString),
            "MsSql" => new SqlConnection(connectionString),
            "PostgreSql" => new NpgsqlConnection(connectionString),
            _ => new NpgsqlConnection(connectionString)
        };

        try
        {
            var evolve = new Evolve(connection, logMessage => { _logger.LogInformation(logMessage); })
            {
                Locations = new[] { sqlPath },
                IsEraseDisabled = true,
                EnableClusterMode = true,
                TransactionMode = EvolveDb.Configuration.TransactionKind.CommitAll,
                OutOfOrder = true,
            };

            evolve.Migrate();
        }
        catch (Exception exception)
        {
            _logger.LogCritical("DbMigrateException : {exception}", exception);
            throw;
        }
    }
}
