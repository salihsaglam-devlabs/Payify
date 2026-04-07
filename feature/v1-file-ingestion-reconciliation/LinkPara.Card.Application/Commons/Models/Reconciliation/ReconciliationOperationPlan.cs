namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationOperationPlan
{
    public Guid RunId { get; set; }
    public Guid CardTransactionRecordId { get; set; }
    public Guid? ClearingRecordId { get; set; }
    public string OceanTxnGuid { get; set; }
    public string OceanMainTxnGuid { get; set; }
    public string CardNo { get; set; }
    public string Rrn { get; set; }
    public string Arn { get; set; }
    public string ProvisionCode { get; set; }
    public string Mcc { get; set; }
    public string TxnStat { get; set; }
    public string ResponseCode { get; set; }
    public string IsSuccessfulTxn { get; set; }
    public string IsTxnSettle { get; set; }
    public string TxnEffect { get; set; }
    public decimal? CardHolderBillingAmount { get; set; }
    public string CardHolderBillingCurrency { get; set; }
    public string[] PlannedOperations { get; set; } = [];
    public Dictionary<string, string> DerivedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
