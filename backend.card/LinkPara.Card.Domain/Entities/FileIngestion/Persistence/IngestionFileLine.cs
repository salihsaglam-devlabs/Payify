using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public class IngestionFileLine : AuditEntity
{
    public Guid FileId { get; set; }
    public IngestionFile IngestionFile { get; set; }
    public long LineNumber { get; set; }
    public long ByteOffset { get; set; }
    public int ByteLength { get; set; }
    public string LineType { get; set; }
    public string RawContent { get; set; }
    public string ParsedContent { get; set; }
    public FileRowStatus Status { get; set; }
    public string Message { get; set; }
    public int RetryCount { get; set; }
    public string CorrelationKey { get; set; }
    public string CorrelationValue { get; set; }
    public string DuplicateDetectionKey { get; set; }
    public string DuplicateStatus { get; set; }
    public Guid? DuplicateGroupId { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public Guid? MatchedClearingLineId { get; set; }
    
    public IngestionCardVisaDetail CardVisaDetail { get; set; }
    public IngestionCardMscDetail CardMscDetail { get; set; }
    public IngestionCardBkmDetail CardBkmDetail { get; set; }
    public IngestionClearingVisaDetail ClearingVisaDetail { get; set; }
    public IngestionClearingMscDetail ClearingMscDetail { get; set; }
    public IngestionClearingBkmDetail ClearingBkmDetail { get; set; }
}
