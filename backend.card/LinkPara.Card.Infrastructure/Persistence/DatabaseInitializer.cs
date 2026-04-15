using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace LinkPara.Card.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task ApplySqlMigrationsAsync(
        IServiceProvider serviceProvider,
        ILogger logger = null)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        var provider = db.Database.ProviderName ?? string.Empty;
        var folder = provider.Contains("Npgsql") ? "PostgreSql" : "MsSql";

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Persistence", "SqlMigrations", folder),
            Path.Combine(Directory.GetCurrentDirectory(), "Persistence", "SqlMigrations", folder),
        };

        string sqlDir = null;
        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath))
            {
                sqlDir = fullPath;
                break;
            }
        }

        if (sqlDir == null)
        {
            var searchedPaths = string.Join(", ", candidates.Select(Path.GetFullPath));
            logger?.LogError("SQL migration directory not found. Searched: {Paths}", searchedPaths);
            Console.WriteLine($"[SqlMigrations] ERROR: SQL migration directory not found. Searched: {searchedPaths}");
            return;
        }

        Console.WriteLine($"[SqlMigrations] Found SQL migration directory: {sqlDir}");

        var sqlFiles = Directory.GetFiles(sqlDir, "*.sql")
            .OrderBy(f => f)
            .ToList();

        Console.WriteLine($"[SqlMigrations] Found {sqlFiles.Count} SQL file(s) to apply.");

        foreach (var file in sqlFiles)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                var sql = await File.ReadAllTextAsync(file);
                await db.Database.ExecuteSqlRawAsync(sql);
                logger?.LogInformation("Applied SQL migration: {File}", fileName);
                Console.WriteLine($"[SqlMigrations] Applied: {fileName}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to apply SQL migration: {File}", fileName);
                Console.WriteLine($"[SqlMigrations] FAILED: {fileName} — {ex.Message}");
                throw;
            }
        }

        Console.WriteLine("[SqlMigrations] All SQL migrations applied successfully.");
    }

    public static async Task EnsureMigrationBaselineAsync(IServiceProvider serviceProvider, ILogger? logger = null)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        var provider = db.Database.ProviderName ?? string.Empty;
        var migrationsAssembly = db.GetService<IMigrationsAssembly>();
        var historyRepository = db.GetService<IHistoryRepository>();
        var pendingMigrations = (await db.Database.GetPendingMigrationsAsync()).ToList();

        foreach (var migrationId in pendingMigrations)
        {
            if (!migrationsAssembly.Migrations.TryGetValue(migrationId, out var migrationType))
                break;

            var migration = migrationsAssembly.CreateMigration(migrationType, provider);
            var createTableOperations = migration.UpOperations.OfType<CreateTableOperation>().ToList();

            if (createTableOperations.Count == 0)
                break;

            var allTablesExist = true;
            foreach (var operation in createTableOperations)
            {
                var schema = operation.Schema ?? GetDefaultSchema(provider);
                if (await CheckTableExistsAsync(db, provider, schema, operation.Name))
                    continue;

                allTablesExist = false;
                break;
            }

            if (!allTablesExist)
                break;

            if (!await historyRepository.ExistsAsync())
                await db.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript());

            var historyRow = new HistoryRow(migrationId, ProductInfo.GetVersion());
            await db.Database.ExecuteSqlRawAsync(historyRepository.GetInsertScript(historyRow));

            logger?.LogWarning(
                "Migration {MigrationId} was baselined because all tables declared in the migration already exist.",
                migrationId);
        }
    }

    public static async Task EnsureTablesExistAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILogger? logger = null)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();

        var provider = db.Database.ProviderName ?? string.Empty;
        logger?.LogInformation("Database provider: {Provider}", provider);

        var script = db.Database.GenerateCreateScript();

        var entityTypes = db.Model.GetEntityTypes()
            .Where(t => t.GetTableName() is not null)
            .ToList();

        foreach (var et in entityTypes)
        {
            var table = et.GetTableName()!;
            var schema = et.GetSchema() ?? (provider.Contains("SqlServer") ? "dbo" : "public");

            var exists = await CheckTableExistsAsync(db, provider, schema, table);
            if (exists)
                continue;

            logger?.LogWarning("Table {Schema}.{Table} is missing. Attempting to create...", schema, table);

            var createStmt = ExtractCreateTableStatement(script, provider, schema, table);
            if (string.IsNullOrWhiteSpace(createStmt))
            {
                logger?.LogError("Could not locate CREATE TABLE statement for {Schema}.{Table} in generated script.", schema, table);
                continue;
            }
            
            var guarded = GuardCreateTableStatement(createStmt, provider, schema, table);

            try
            {
                await db.Database.ExecuteSqlRawAsync(guarded);
                logger?.LogInformation("Created table {Schema}.{Table}.", schema, table);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to create table {Schema}.{Table}.", schema, table);
            }
        }
    }

    private static async Task<bool> CheckTableExistsAsync(DbContext db, string provider, string schema, string table)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();
        
        using var cmd = conn.CreateCommand();

        if (provider.Contains("Npgsql"))
        {
            cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema = @p0 AND table_name = @p1);";
        }
        else if (provider.Contains("SqlServer"))
        {
            cmd.CommandText = "SELECT CASE WHEN EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @p0 AND TABLE_NAME = @p1) THEN 1 ELSE 0 END;";
        }
        else
        {
            cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema = @p0 AND table_name = @p1);";
        }

        var p0 = cmd.CreateParameter();
        p0.ParameterName = "@p0";
        p0.Value = schema;
        cmd.Parameters.Add(p0);

        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@p1";
        p1.Value = table;
        cmd.Parameters.Add(p1);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null)
            return false;

        if (provider.Contains("Npgsql"))
            return result is bool b && b;

        return Convert.ToInt32(result) == 1;
    }

    private static string GetDefaultSchema(string provider)
    {
        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            return "dbo";

        return "public";
    }

    private static string? ExtractCreateTableStatement(string createScript, string provider, string schema, string table)
    {
        var normalized = Regex.Replace(createScript, "\r\n|\r|\n", "\n");

        var searchNameCandidates = new[]
        {
            $"{schema}.{table}",
            $"\"{schema}\".\"{table}\"",
            $"[{schema}].[{table}]",
            $"{table}"
        };

        foreach (var candidate in searchNameCandidates)
        {
            var idx = normalized.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;
            
            var createIdx = normalized.LastIndexOf("CREATE TABLE", idx, StringComparison.OrdinalIgnoreCase);
            if (createIdx < 0) continue;
            
            var semicolonIdx = normalized.IndexOf(';', createIdx);
            string block;
            if (semicolonIdx > createIdx)
                block = normalized.Substring(createIdx, semicolonIdx - createIdx + 1);
            else
            {
                var nextCreate = normalized.IndexOf("CREATE TABLE", createIdx + 1, StringComparison.OrdinalIgnoreCase);
                if (nextCreate > createIdx)
                    block = normalized.Substring(createIdx, nextCreate - createIdx);
                else
                    block = normalized.Substring(createIdx);
            }

            return block.Trim();
        }

        return null;
    }

    private static string GuardCreateTableStatement(string createStmt, string provider, string schema, string table)
    {
        if (provider.Contains("Npgsql"))
        {
            var pattern = "CREATE TABLE";
            var replaced = Regex.Replace(createStmt, pattern, "CREATE TABLE IF NOT EXISTS", RegexOptions.IgnoreCase);
            return replaced;
        }
        else if (provider.Contains("SqlServer"))
        {
            var guard = $@"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{table}')
            BEGIN
            {createStmt}
            END";
            return guard;
        }
        return createStmt;
    }
}
