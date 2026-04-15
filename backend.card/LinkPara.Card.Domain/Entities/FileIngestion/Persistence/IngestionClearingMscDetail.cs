using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public class IngestionClearingMscDetail : AuditEntity, IIngestionTypedDetail
{
    public Guid IngestionFileLineId { get; set; }
    public IngestionFileLine IngestionFileLine { get; set; }

    // ClearingMscDetail fields
    public ClearingMscTxnType TxnType { get; set; }
    public string IoDate { get; set; } = default!;
    public ClearingMscIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ReasonCode { get; set; } = default!;
    public string Reserved { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public string CardNo { get; set; } = default!;
    public ClearingMscCardDci? CardDci { get; set; }
    public string MccCode { get; set; } = default!;
    public string Mtid { get; set; } = default!;
    public string FunctionCode { get; set; } = default!;
    public string ProcessCode { get; set; } = default!;
    public string ReversalIndicator { get; set; } = default!;
    public ClearingMscDisputeCode DisputeCode { get; set; }
    public ClearingMscControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; } = default!;
    public string AncillaryTransactionCode { get; set; } = default!;
    public string AncillaryTransactionAmount { get; set; } = default!;
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string CardAcceptorId { get; set; } = default!;
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; } = default!;
}

