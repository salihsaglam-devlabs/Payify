using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities
{
    public class DebitAuthorization : AuditEntity
    {
        public long CorrelationID { get; set; }
        public long OceanTxnGUID { get; set; }
        public string BankingCustomerNo { get; set; }
        public string CardNo { get; set; }
        public string AccountNo { get; set; }
        public string AccountBranch { get; set; }
        public string AccountSuffix { get; set; }
        public int? AccountCurrency { get; set; }
        public string Iban { get; set; }
        public string AcquirerCountryCode { get; set; }
        public string NationalSwitchId { get; set; }
        public string AcquirerId { get; set; }
        public string TerminalId { get; set; }
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string Rrn { get; set; }
        public string ProvisionCode { get; set; }
        public decimal TransactionAmount { get; set; }
        public int TransactionCurrency{ get; set; }
        public decimal BillingAmount { get; set; }
        public int BillingCurrency { get; set; }
        public decimal? ReplacementTransactionAmount { get; set; }
        public int? ReplacementTransactionCurrency { get; set; }
        public decimal? ReplacementBillingAmount { get; set; }
        public int? ReplacementBillingCurrency { get; set; }
        public long RequestDate { get; set; }
        public long RequestTime { get; set; }
        public string Mcc { get; set; }
        public bool IsSimulation { get; set; }
        public bool IsAdvice { get; set; }
        public string RequestType { get; set; }
        public string TransactionType { get; set; }
        public int? ExpirationTime { get; set; }
        public string Channel { get; set; }
        public string TerminalType { get; set; }
        public string BankingRefNo { get; set; }
        public char TransactionSource { get; set; }
        public char CardDci { get; set; }
        public char CardBrand { get; set; }
        public char EntryType { get; set; }
        public bool? PartialAcceptor { get; set; }
        public char? TransferInformationType { get; set; }
        public string TransferInformationName { get; set; }
        public string TransferInformationCardNo{ get; set; }
        public string BusinesssApplicationIdentifier { get; set; }
        public string QRData { get; set; }
        public int? SecurityLevelIndicator { get; set; }
        public bool IsReturn { get; set; }
        }
}
