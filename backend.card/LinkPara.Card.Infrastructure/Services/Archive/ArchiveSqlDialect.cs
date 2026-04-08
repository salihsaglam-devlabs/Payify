using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal interface IArchiveSqlDialect
{
    string BuildCopyIngestionFileSql();
    string BuildCopyIngestionFileLineSql();
    string BuildCopyReconciliationEvaluationSql();
    string BuildCopyReconciliationOperationSql();
    string BuildCopyReconciliationReviewSql();
    string BuildCopyReconciliationOperationExecutionSql();
    string BuildCopyReconciliationAlertSql();
    string BuildDeleteReconciliationAlertSql();
    string BuildDeleteReconciliationOperationExecutionSql();
    string BuildDeleteReconciliationReviewSql();
    string BuildDeleteReconciliationOperationSql();
    string BuildDeleteReconciliationEvaluationSql();
    string BuildDeleteIngestionFileLineSql();
    string BuildDeleteIngestionFileSql();
}

/// <summary>
/// Generates archive copy/delete SQL using only live entity EF metadata.
/// Archive business tables mirror the live tables with one extra column: archived_at.
/// No duplicate archive entity types are needed.
/// </summary>
internal sealed class ArchiveSqlDialect : IArchiveSqlDialect
{
    private readonly CardDbContext _dbContext;
    private readonly bool _isSqlServer;

    private static readonly Dictionary<Type, (string Schema, string Table)> ArchiveTableMap = new()
    {
        [typeof(IngestionFile)] = ("archive", "ingestion_file"),
        [typeof(IngestionFileLine)] = ("archive", "ingestion_file_line"),
        [typeof(ReconciliationEvaluation)] = ("archive", "reconciliation_evaluation"),
        [typeof(ReconciliationOperation)] = ("archive", "reconciliation_operation"),
        [typeof(ReconciliationReview)] = ("archive", "reconciliation_review"),
        [typeof(ReconciliationOperationExecution)] = ("archive", "reconciliation_operation_execution"),
        [typeof(ReconciliationAlert)] = ("archive", "reconciliation_alert"),
    };

    public ArchiveSqlDialect(CardDbContext dbContext)
    {
        _dbContext = dbContext;
        _isSqlServer = (_dbContext.Database.ProviderName ?? string.Empty)
            .Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
    }

    // Copy SQL: parameters are {0}=archivedAt, {1}=ingestionFileId

    public string BuildCopyIngestionFileSql()
        => BuildCopySql<IngestionFile>(
            "s",
            $"FROM {GetLiveTableName<IngestionFile>()} s",
            "WHERE s.id = {1}");

    public string BuildCopyIngestionFileLineSql()
        => BuildCopySql<IngestionFileLine>(
            "s",
            $"FROM {GetLiveTableName<IngestionFileLine>()} s",
            "WHERE s.file_id = {1}");

    public string BuildCopyReconciliationEvaluationSql()
        => BuildCopySql<ReconciliationEvaluation>(
            "s",
            $"FROM {GetLiveTableName<ReconciliationEvaluation>()} s JOIN {GetLiveTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationOperationSql()
        => BuildCopySql<ReconciliationOperation>(
            "s",
            $"FROM {GetLiveTableName<ReconciliationOperation>()} s JOIN {GetLiveTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationReviewSql()
        => BuildCopySql<ReconciliationReview>(
            "s",
            $"FROM {GetLiveTableName<ReconciliationReview>()} s JOIN {GetLiveTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationOperationExecutionSql()
        => BuildCopySql<ReconciliationOperationExecution>(
            "s",
            $"FROM {GetLiveTableName<ReconciliationOperationExecution>()} s JOIN {GetLiveTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationAlertSql()
        => BuildCopySql<ReconciliationAlert>(
            "s",
            $"FROM {GetLiveTableName<ReconciliationAlert>()} s JOIN {GetLiveTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    // Delete SQL: parameter is {0}=ingestionFileId (unchanged)

    public string BuildDeleteReconciliationAlertSql()
        => $"DELETE FROM {GetLiveTableName<ReconciliationAlert>()} WHERE file_line_id IN (SELECT id FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationExecutionSql()
        => $"DELETE FROM {GetLiveTableName<ReconciliationOperationExecution>()} WHERE file_line_id IN (SELECT id FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationReviewSql()
        => $"DELETE FROM {GetLiveTableName<ReconciliationReview>()} WHERE file_line_id IN (SELECT id FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationSql()
        => $"DELETE FROM {GetLiveTableName<ReconciliationOperation>()} WHERE file_line_id IN (SELECT id FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationEvaluationSql()
        => $"DELETE FROM {GetLiveTableName<ReconciliationEvaluation>()} WHERE file_line_id IN (SELECT id FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteIngestionFileLineSql()
        => $"DELETE FROM {GetLiveTableName<IngestionFileLine>()} WHERE file_id = {{0}};";

    public string BuildDeleteIngestionFileSql()
        => $"DELETE FROM {GetLiveTableName<IngestionFile>()} WHERE id = {{0}};";

    /// <summary>
    /// Builds INSERT INTO archive.{table} ({live_columns}, archived_at)
    /// SELECT {live_columns}, {0} FROM {live_table} ...
    /// Parameters: {0}=archivedAt, {1}=ingestionFileId
    /// </summary>
    private string BuildCopySql<TSource>(string sourceAlias, string fromClause, string whereClause)
        where TSource : class
    {
        var sourceEntity = _dbContext.Model.FindEntityType(typeof(TSource))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TSource).Name}.");

        var storeColumns = GetStoreColumnNames(sourceEntity);
        var archiveTable = GetArchiveTableName<TSource>();

        var insertColumns = storeColumns
            .Concat(new[] { QuoteIdentifier("archived_at") });

        var selectColumns = storeColumns
            .Select(col => $"{sourceAlias}.{col}")
            .Concat(new[] { "{0}" });

        return $"""
            INSERT INTO {archiveTable}
            (
                {string.Join(", ", insertColumns)}
            )
            SELECT
                {string.Join(", ", selectColumns)}
            {fromClause}
            {whereClause}
            """;
    }

    private List<string> GetStoreColumnNames(IEntityType entityType)
    {
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        return entityType.GetProperties()
            .Select(p => QuoteIdentifier(p.GetColumnName(storeObject)!))
            .ToList();
    }

    private string GetArchiveTableName<TSource>() where TSource : class
    {
        if (!ArchiveTableMap.TryGetValue(typeof(TSource), out var mapping))
            throw new InvalidOperationException($"No archive table mapping for {typeof(TSource).Name}.");
        return $"{QuoteIdentifier(mapping.Schema)}.{QuoteIdentifier(mapping.Table)}";
    }

    private string GetLiveTableName<TEntity>() where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TEntity).Name}.");

        var schema = entityType.GetSchema();
        var table = entityType.GetTableName()!;
        return string.IsNullOrWhiteSpace(schema)
            ? QuoteIdentifier(table)
            : $"{QuoteIdentifier(schema)}.{QuoteIdentifier(table)}";
    }

    private string QuoteIdentifier(string name)
    {
        return _isSqlServer ? $"[{name}]" : $"\"{name}\"";
    }
}
