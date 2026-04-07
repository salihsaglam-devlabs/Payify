using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class ClearingRecord : AuditEntity
{
    public Guid ImportedFileRowId { get; set; }

    public ClearingProvider Provider { get; set; }
    public string TxnType { get; set; }
    public DateOnly? IoDate { get; set; }
    public string IoFlag { get; set; }
    public string OceanTxnGuid { get; set; }
    public string ClrNo { get; set; }
    public string Rrn { get; set; }
    public string Arn { get; set; }
    public string ReasonCode { get; set; }
    public string Reserved { get; set; }
    public string ProvisionCode { get; set; }
    public string CardNo { get; set; }
    public string CardDci { get; set; }
    public string MccCode { get; set; }
    public string Mtid { get; set; }
    public string FunctionCode { get; set; }
    public string ProcessCode { get; set; }
    public string ReversalIndicator { get; set; }
    public string Tc { get; set; }
    public string UsageCode { get; set; }
    public string DisputeCode { get; set; }
    public string ControlStat { get; set; }
    public decimal? SourceAmount { get; set; }
    public string SourceCurrency { get; set; }
    public decimal? DestinationAmount { get; set; }
    public string DestinationCurrency { get; set; }
    public decimal? CashbackAmount { get; set; }
    public decimal? ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; }
    public string AncillaryTransactionCode { get; set; }
    public string AncillaryTransactionAmount { get; set; }
    public string MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; }
    public string MerchantName { get; set; }
    public string CardAcceptorId { get; set; }
    public DateOnly? TxnDate { get; set; }
    public TimeOnly? TxnTime { get; set; }
    public string FileId { get; set; }

    public string CorrelationKey { get; set; }

    public ImportedFileRow ImportedFileRow { get; set; }
}
