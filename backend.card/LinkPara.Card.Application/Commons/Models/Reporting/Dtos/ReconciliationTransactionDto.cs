namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class ReconciliationTransactionDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public Guid CardFileLineId { get; set; }
    public Guid? ClearingFileLineId { get; set; }
    public string Network { get; set; }
    
    public long CardOceanTxnGuid { get; set; }
    public string CardRrn { get; set; }
    public string CardArn { get; set; }
    public string CardCardNo { get; set; }
    public string CardMerchantName { get; set; }
    public int CardTransactionDate { get; set; }
    public decimal CardOriginalAmount { get; set; }
    public int CardOriginalCurrency { get; set; }
    public decimal CardSettlementAmount { get; set; }
    public int CardSettlementCurrency { get; set; }
    public decimal CardBillingAmount { get; set; }
    public string CardIsSuccessfulTxn { get; set; }

    public long? ClearingOceanTxnGuid { get; set; }
    public string ClearingRrn { get; set; }
    public string ClearingArn { get; set; }
    public string ClearingCardNo { get; set; }
    public string ClearingMerchantName { get; set; }
    public int? ClearingTxnDate { get; set; }
    public decimal? ClearingSourceAmount { get; set; }
    public int? ClearingSourceCurrency { get; set; }
    public decimal? ClearingDestinationAmount { get; set; }
    public int? ClearingDestinationCurrency { get; set; }
    public string ClearingControlStat { get; set; }
    
    public string MatchStatus { get; set; }
    
    public decimal? AmountDifference { get; set; }
    public bool? HasAmountMismatch { get; set; }
    public bool? HasCurrencyMismatch { get; set; }
    public bool? HasDateMismatch { get; set; }
    public bool? HasStatusMismatch { get; set; }
    
    public string ReconciliationStatus { get; set; }
    public string DuplicateStatus { get; set; }
    
    public DateTime CardCreateDate { get; set; }
}

