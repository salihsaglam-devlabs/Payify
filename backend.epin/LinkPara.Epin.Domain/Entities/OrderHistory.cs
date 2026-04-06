using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class OrderHistory : AuditEntity
{
    public int ExternalId { get; set; }
    public decimal Total { get; set; }
    public string Pin { get; set; }
    public int ExternalProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public DateTime TransactionDate { get; set; }
}
