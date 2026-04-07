using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class ImportedFileRow : AuditEntity
{
    public Guid ImportedFileId { get; set; }
    public int LineNo { get; set; }
    public string RowType { get; set; }
    public string RawLine { get; set; }
    public string ParsedJson { get; set; }

    public ImportedFile ImportedFile { get; set; }
    public CardTransactionRecord CardTransactionRecord { get; set; }
    public ClearingRecord ClearingRecord { get; set; }
}
