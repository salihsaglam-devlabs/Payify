using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;
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

    public string BuildCopyIngestionFileSql()
        => BuildCopySql<IngestionFile, ArchiveIngestionFile>(
            "s",
            $"FROM {GetQualifiedTableName<IngestionFile>()} s",
            "WHERE s.id = {3}");

    public string BuildCopyIngestionFileLineSql()
        => BuildCopySql<IngestionFileLine, ArchiveIngestionFileLine>(
            "s",
            $"FROM {GetQualifiedTableName<IngestionFileLine>()} s",
            "WHERE s.file_id = {3}");

    public string BuildCopyReconciliationEvaluationSql()
        => BuildCopySql<ReconciliationEvaluation, ArchiveReconciliationEvaluation>(
            "s",
            $"FROM {GetQualifiedTableName<ReconciliationEvaluation>()} s JOIN {GetQualifiedTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {3}");

    public string BuildCopyReconciliationOperationSql()
        => BuildCopySql<ReconciliationOperation, ArchiveReconciliationOperation>(
            "s",
            $"FROM {GetQualifiedTableName<ReconciliationOperation>()} s JOIN {GetQualifiedTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {3}");

    public string BuildCopyReconciliationReviewSql()
        => BuildCopySql<ReconciliationReview, ArchiveReconciliationReview>(
            "s",
            $"FROM {GetQualifiedTableName<ReconciliationReview>()} s JOIN {GetQualifiedTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {3}");

    public string BuildCopyReconciliationOperationExecutionSql()
        => BuildCopySql<ReconciliationOperationExecution, ArchiveReconciliationOperationExecution>(
            "s",
            $"FROM {GetQualifiedTableName<ReconciliationOperationExecution>()} s JOIN {GetQualifiedTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {3}");

    public string BuildCopyReconciliationAlertSql()
        => BuildCopySql<ReconciliationAlert, ArchiveReconciliationAlert>(
            "s",
            $"FROM {GetQualifiedTableName<ReconciliationAlert>()} s JOIN {GetQualifiedTableName<IngestionFileLine>()} l ON l.id = s.file_line_id",
            "WHERE l.file_id = {3}");

    public string BuildDeleteReconciliationAlertSql()
        => $"DELETE FROM {GetQualifiedTableName<ReconciliationAlert>()} WHERE file_line_id IN (SELECT id FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationExecutionSql()
        => $"DELETE FROM {GetQualifiedTableName<ReconciliationOperationExecution>()} WHERE file_line_id IN (SELECT id FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationReviewSql()
        => $"DELETE FROM {GetQualifiedTableName<ReconciliationReview>()} WHERE file_line_id IN (SELECT id FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationOperationSql()
        => $"DELETE FROM {GetQualifiedTableName<ReconciliationOperation>()} WHERE file_line_id IN (SELECT id FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteReconciliationEvaluationSql()
        => $"DELETE FROM {GetQualifiedTableName<ReconciliationEvaluation>()} WHERE file_line_id IN (SELECT id FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}});";

    public string BuildDeleteIngestionFileLineSql()
        => $"DELETE FROM {GetQualifiedTableName<IngestionFileLine>()} WHERE file_id = {{0}};";

    public string BuildDeleteIngestionFileSql()
        => $"DELETE FROM {GetQualifiedTableName<IngestionFile>()} WHERE id = {{0}};";

    private string BuildCopySql<TSource, TArchive>(string sourceAlias, string fromClause, string whereClause)
        where TSource : class
        where TArchive : class
    {
        var sourceEntity = _dbContext.Model.FindEntityType(typeof(TSource))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TSource).Name}.");
        var archiveEntity = _dbContext.Model.FindEntityType(typeof(TArchive))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TArchive).Name}.");

        var sourceColumns = GetStoreColumns(sourceEntity);
        var archiveColumns = GetStoreColumns(archiveEntity);

        var archivePropertyNames = archiveEntity.GetProperties()
            .Select(x => x.Name)
            .ToList();

        var sharedPropertyNames = archivePropertyNames
            .Where(x => x is not nameof(ArchiveIngestionFile.ArchiveRunId)
                and not nameof(ArchiveIngestionFile.ArchivedAt)
                and not nameof(ArchiveIngestionFile.ArchivedBy))
            .Where(sourceColumns.ContainsKey)
            .ToList();

        var insertColumns = sharedPropertyNames
            .Select(name => archiveColumns[name])
            .Concat(new[]
            {
                archiveColumns[nameof(ArchiveIngestionFile.ArchiveRunId)],
                archiveColumns[nameof(ArchiveIngestionFile.ArchivedAt)],
                archiveColumns[nameof(ArchiveIngestionFile.ArchivedBy)]
            });

        var selectColumns = sharedPropertyNames
            .Select(name => $"{sourceAlias}.{sourceColumns[name]}")
            .Concat(new[] { "{0}", "{1}", "{2}" });

        return $"""
            INSERT INTO {GetQualifiedTableName<TArchive>()}
            (
                {string.Join(", ", insertColumns)}
            )
            SELECT
                {string.Join(", ", selectColumns)}
            {fromClause}
            {whereClause}
            """;
    }

    private Dictionary<string, string> GetStoreColumns(IEntityType entityType)
    {
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        return entityType.GetProperties().ToDictionary(
            property => property.Name,
            property => QuoteIdentifier(property.GetColumnName(storeObject)!));
    }

    private string GetQualifiedTableName<TEntity>() where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity type not found for {typeof(TEntity).Name}.");

        return GetQualifiedTableName(entityType);
    }

    private string GetQualifiedTableName(IEntityType entityType)
    {
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
