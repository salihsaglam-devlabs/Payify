using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public class IngestionClearingVisaDetail : AuditEntity, IIngestionTypedDetail
{
    public Guid IngestionFileLineId { get; set; }
    public IngestionFileLine IngestionFileLine { get; set; }

    // ClearingVisaDetail fields
    public ClearingVisaTxnType TxnType { get; set; }
    public string IoDate { get; set; } = default!;
    public ClearingVisaIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ReasonCode { get; set; } = default!;
    public string Reserved { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public string CardNo { get; set; } = default!;
    public ClearingVisaCardDci? CardDci { get; set; }
    public string MccCode { get; set; } = default!;
    public string Tc { get; set; } = default!;
    public string UsageCode { get; set; } = default!;
    public ClearingVisaDisputeCode DisputeCode { get; set; }
    public ClearingVisaControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; } = default!;
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string CardAcceptorId { get; set; } = default!;
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; } = default!;
}

