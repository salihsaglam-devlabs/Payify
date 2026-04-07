using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class ImportedFile : AuditEntity
{
    public string FileName { get; set; }
    public FileFamily FileFamily { get; set; }
    public string FileType { get; set; }
    public string SourceType { get; set; }
    public string SourcePath { get; set; }
    public string FileHash { get; set; }
    public ImportedFileStatus Status { get; set; }
    public int TotalLineCount { get; set; }
    public string HeaderCode { get; set; }
    public string FooterCode { get; set; }
    public string DeclaredFileDate { get; set; }
    public string DeclaredFileNo { get; set; }
    public string DeclaredFileVersionNumber { get; set; }
    public string DeclaredTxnCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string ErrorMessage { get; set; }

    public ICollection<ImportedFileRow> Rows { get; set; } = new List<ImportedFileRow>();
}
