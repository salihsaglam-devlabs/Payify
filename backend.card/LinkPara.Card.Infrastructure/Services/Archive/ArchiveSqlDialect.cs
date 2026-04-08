using LinkPara.Card.Domain.Entities.Archive;
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
/// Generates archive copy/delete SQL using EF metadata for both live and archive entity types.
/// Archive entities inherit from live entities and are mapped to the archive schema.
/// No hardcoded table names; all resolved from EF model metadata.
/// </summary>
internal sealed class ArchiveSqlDialect : IArchiveSqlDialect
{
    private readonly CardDbContext _dbContext;
    private readonly bool _isSqlServer;

    public ArchiveSqlDialect(CardDbContext dbContext)
    {
        _dbContext = dbContext;
        _isSqlServer = (_dbContext.Database.ProviderName ?? string.Empty)
            .Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
    }

    // Copy SQL: parameters are {0}=archivedAt, {1}=ingestionFileId

    public string BuildCopyIngestionFileSql()
        => BuildCopySql<IngestionFile, ArchiveIngestionFile>(
            "s",
            $"FROM {GetTableName<IngestionFile>()} s",
            "WHERE s.id = {1}");

    public string BuildCopyIngestionFileLineSql()
        => BuildCopySql<IngestionFileLine, ArchiveIngestionFileLine>(
            "s",
            $"FROM {GetTableName<IngestionFileLine>()} s",
            "WHERE s.file_id = {1}");

    public string BuildCopyReconciliationEvaluationSql()
        => BuildCopySql<ReconciliationEvaluation, ArchiveReconciliationEvaluation>(
            "s",
            $"FROM {GetTableName<ReconciliationEvaluation>()} s JOIN {GetTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationOperationSql()
        => BuildCopySql<ReconciliationOperation, ArchiveReconciliationOperation>(
            "s",
            $"FROM {GetTableName<ReconciliationOperation>()} s JOIN {GetTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationReviewSql()
        => BuildCopySql<ReconciliationReview, ArchiveReconciliationReview>(
            "s",
            $"FROM {GetTableName<ReconciliationReview>()} s JOIN {GetTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationOperationExecutionSql()
        => BuildCopySql<ReconciliationOperationExecution, ArchiveReconciliationOperationExecution>(
            "s",
            $"FROM {GetTableName<ReconciliationOperationExecution>()} s JOIN {GetTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    public string BuildCopyReconciliationAlertSql()
        => BuildCopySql<ReconciliationAlert, ArchiveReconciliationAlert>(
            "s",
            $"FROM {GetTableName<ReconciliationAlert>()} s JOIN {GetTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {1}");

    // Delete SQL: parameter is {0}=ingestionFileId (unchanged)

    public string BuildDeleteReconciliationAlertSql()
        => $"DELETE FROM {GetTableName<ReconciliationAlert>()} WHERE file_line_id IN (SELECT id FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationExecutionSql()
        => $"DELETE FROM {GetTableName<ReconciliationOperationExecution>()} WHERE file_line_id IN (SELECT id FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationReviewSql()
        => $"DELETE FROM {GetTableName<ReconciliationReview>()} WHERE file_line_id IN (SELECT id FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationSql()
        => $"DELETE FROM {GetTableName<ReconciliationOperation>()} WHERE file_line_id IN (SELECT id FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationEvaluationSql()
        => $"DELETE FROM {GetTableName<ReconciliationEvaluation>()} WHERE file_line_id IN (SELECT id FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteIngestionFileLineSql()
        => $"DELETE FROM {GetTableName<IngestionFileLine>()} WHERE file_id = {{0}};";

    public string BuildDeleteIngestionFileSql()
        => $"DELETE FROM {GetTableName<IngestionFile>()} WHERE id = {{0}};";

    /// <summary>
    /// Builds INSERT INTO archive_table ({live_columns}, archived_at)
    /// SELECT {live_columns}, {0} FROM {live_table} ...
    /// Parameters: {0}=archivedAt, {1}=ingestionFileId
    /// </summary>
    private string BuildCopySql<TSource, TArchive>(string sourceAlias, string fromClause, string whereClause)
        where TSource : class
        where TArchive : class
    {
        var sourceEntity = _dbContext.Model.FindEntityType(typeof(TSource))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TSource).Name}.");

        var storeColumns = GetStoreColumnNames(sourceEntity);
        var archiveTable = GetTableName<TArchive>();

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

    private string GetTableName<TEntity>() where TEntity : class
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
