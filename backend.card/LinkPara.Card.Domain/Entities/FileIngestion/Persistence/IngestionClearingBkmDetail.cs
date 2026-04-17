using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public class IngestionClearingBkmDetail : AuditEntity, IIngestionTypedDetail,IClearingBkmDetail
{
    public Guid FileLineId { get; set; }
    public IngestionFileLine FileLine { get; set; }

    public ClearingBkmTxnType TxnType { get; set; }
    public string IoDate { get; set; }
    public ClearingBkmIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; }
    public string Arn { get; set; }
    public string ReasonCode { get; set; }
    public string Reserved { get; set; }
    public string ProvisionCode { get; set; }
    public string CardNo { get; set; }
    public ClearingBkmCardDci? CardDci { get; set; }
    public string MccCode { get; set; }
    public string Mtid { get; set; }
    public string FunctionCode { get; set; }
    public string ProcessCode { get; set; }
    public ClearingBkmDisputeCode DisputeCode { get; set; }
    public ClearingBkmControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; }
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; }
    public string MerchantName { get; set; }
    public string CardAcceptorId { get; set; }
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; }
}

