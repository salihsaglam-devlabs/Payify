using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities
{
    public class DebitAuthorizationFee : AuditEntity
    {
        public long OceanTxnGUID { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyCode { get; set; }
        public decimal Tax1Amount { get; set; }
        public decimal Tax2Amount { get; set; }
    }
}