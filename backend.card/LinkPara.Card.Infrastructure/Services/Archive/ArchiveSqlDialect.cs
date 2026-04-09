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

internal sealed class ArchiveSqlDialect : IArchiveSqlDialect
{
    private readonly CardDbContext _dbContext;
    private readonly bool _isSqlServer;
    
    private const string FileLineIdProperty = nameof(ReconciliationEvaluation.FileLineId);
    private const string IngestionFileIdProperty = nameof(IngestionFileLine.IngestionFileId);
    
    private static readonly (string PropertyName, string ParamPlaceholder)[] ArchiveOnlyColumns =
    [
        (nameof(ArchiveIngestionFile.ArchivedAt), "{0}"),
        (nameof(ArchiveIngestionFile.ArchivedBy), "{1}"),
        (nameof(ArchiveIngestionFile.ArchiveRunId), "{2}")
    ];

    private const string FilterParamPlaceholder = "{3}";

    public ArchiveSqlDialect(CardDbContext dbContext)
    {
        _dbContext = dbContext;
        _isSqlServer = (_dbContext.Database.ProviderName ?? string.Empty)
            .Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
    }
    
    public string BuildCopyIngestionFileSql()
    {
        var pk = Pk<IngestionFile>();
        return BuildCopySql<IngestionFile, ArchiveIngestionFile>(
            "s",
            $"FROM {Tbl<IngestionFile>()} s",
            $"WHERE s.{pk} = {FilterParamPlaceholder}");
    }

    public string BuildCopyIngestionFileLineSql()
    {
        var fk = Col<IngestionFileLine>(IngestionFileIdProperty);
        return BuildCopySql<IngestionFileLine, ArchiveIngestionFileLine>(
            "s",
            $"FROM {Tbl<IngestionFileLine>()} s",
            $"WHERE s.{fk} = {FilterParamPlaceholder}");
    }

    public string BuildCopyReconciliationEvaluationSql()
        => BuildReconCopySql<ReconciliationEvaluation, ArchiveReconciliationEvaluation>();

    public string BuildCopyReconciliationOperationSql()
        => BuildReconCopySql<ReconciliationOperation, ArchiveReconciliationOperation>();

    public string BuildCopyReconciliationReviewSql()
        => BuildReconCopySql<ReconciliationReview, ArchiveReconciliationReview>();

    public string BuildCopyReconciliationOperationExecutionSql()
        => BuildReconCopySql<ReconciliationOperationExecution, ArchiveReconciliationOperationExecution>();

    public string BuildCopyReconciliationAlertSql()
        => BuildReconCopySql<ReconciliationAlert, ArchiveReconciliationAlert>();
    
    public string BuildDeleteReconciliationAlertSql()
        => BuildReconDeleteSql<ReconciliationAlert>();

    public string BuildDeleteReconciliationOperationExecutionSql()
        => BuildReconDeleteSql<ReconciliationOperationExecution>();

    public string BuildDeleteReconciliationReviewSql()
        => BuildReconDeleteSql<ReconciliationReview>();

    public string BuildDeleteReconciliationOperationSql()
        => BuildReconDeleteSql<ReconciliationOperation>();

    public string BuildDeleteReconciliationEvaluationSql()
        => BuildReconDeleteSql<ReconciliationEvaluation>();

    public string BuildDeleteIngestionFileLineSql()
    {
        var fk = Col<IngestionFileLine>(IngestionFileIdProperty);
        return $"DELETE FROM {Tbl<IngestionFileLine>()} WHERE {fk} = {{0}};";
    }

    public string BuildDeleteIngestionFileSql()
    {
        var pk = Pk<IngestionFile>();
        return $"DELETE FROM {Tbl<IngestionFile>()} WHERE {pk} = {{0}};";
    }
    
    private string BuildReconCopySql<TSource, TArchive>()
        where TSource : class
        where TArchive : class
    {
        var sourceFileLineId = Col<TSource>(FileLineIdProperty);
        var fileLinePk = Pk<IngestionFileLine>();
        var fileLineFileId = Col<IngestionFileLine>(IngestionFileIdProperty);
        var fileLineTable = Tbl<IngestionFileLine>();

        return BuildCopySql<TSource, TArchive>(
            "s",
            $"FROM {Tbl<TSource>()} s JOIN {fileLineTable} l ON l.{fileLinePk} = s.{sourceFileLineId}",
            $"WHERE l.{fileLineFileId} = {FilterParamPlaceholder}");
    }
    
    private string BuildReconDeleteSql<TEntity>() where TEntity : class
    {
        var fileLineIdCol = Col<TEntity>(FileLineIdProperty);
        var fileLinePk = Pk<IngestionFileLine>();
        var fileLineTable = Tbl<IngestionFileLine>();
        var fileLineFileId = Col<IngestionFileLine>(IngestionFileIdProperty);

        return $"DELETE FROM {Tbl<TEntity>()} WHERE {fileLineIdCol} IN (SELECT {fileLinePk} FROM {fileLineTable} WHERE {fileLineFileId} = {{0}});";
    }
    
    private string BuildCopySql<TSource, TArchive>(string sourceAlias, string fromClause, string whereClause)
        where TSource : class
        where TArchive : class
    {
        var sourceEntity = FindEntityType<TSource>();
        var archiveEntity = FindEntityType<TArchive>();

        var sourceColumns = GetStoreColumnNames(sourceEntity);
        var sourceColumnSet = new HashSet<string>(sourceColumns, StringComparer.OrdinalIgnoreCase);

        var archiveTable = Tbl<TArchive>();
        var archiveStoreObject = GetStoreObject(archiveEntity);
        
        var archiveOnlyInsertCols = new List<string>();
        var archiveOnlySelectExprs = new List<string>();

        foreach (var (propertyName, paramPlaceholder) in ArchiveOnlyColumns)
        {
            var prop = archiveEntity.FindProperty(propertyName);
            if (prop is null) continue;

            var colName = QuoteIdentifier(prop.GetColumnName(archiveStoreObject)!);
            if (sourceColumnSet.Contains(colName)) continue;

            archiveOnlyInsertCols.Add(colName);
            archiveOnlySelectExprs.Add(paramPlaceholder);
        }

        var insertColumns = sourceColumns.Concat(archiveOnlyInsertCols);
        var selectColumns = sourceColumns.Select(col => $"{sourceAlias}.{col}").Concat(archiveOnlySelectExprs);

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
        var storeObject = GetStoreObject(entityType);
        return entityType.GetProperties()
            .Select(p => QuoteIdentifier(p.GetColumnName(storeObject)!))
            .ToList();
    }

    private string Tbl<TEntity>() where TEntity : class
    {
        var entityType = FindEntityType<TEntity>();
        var schema = entityType.GetSchema();
        var table = entityType.GetTableName()!;
        return string.IsNullOrWhiteSpace(schema)
            ? QuoteIdentifier(table)
            : $"{QuoteIdentifier(schema)}.{QuoteIdentifier(table)}";
    }

    private string Col<TEntity>(string propertyName) where TEntity : class
    {
        var entityType = FindEntityType<TEntity>();
        var property = entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' not found on entity type {typeof(TEntity).Name}.");
        return QuoteIdentifier(property.GetColumnName(GetStoreObject(entityType))!);
    }

    private string Pk<TEntity>() where TEntity : class
    {
        var entityType = FindEntityType<TEntity>();
        var pk = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                $"No primary key defined on entity type {typeof(TEntity).Name}.");
        return QuoteIdentifier(pk.Properties[0].GetColumnName(GetStoreObject(entityType))!);
    }

    private IEntityType FindEntityType<TEntity>() where TEntity : class
    {
        return _dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity type not found in EF model: {typeof(TEntity).Name}.");
    }

    private static StoreObjectIdentifier GetStoreObject(IEntityType entityType)
    {
        return StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
    }

    private string QuoteIdentifier(string name)
    {
        return _isSqlServer ? $"[{name}]" : $"\"{name}\"";
    }
}
