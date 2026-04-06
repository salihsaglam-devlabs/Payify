using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class Product : AuditEntity
{
    public int ExternalId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal UnitPrice { get; set; }
    public string Equivalent { get; set; }
    public string Vat { get; set; }
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; }
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; }
    public decimal Discount { get; set; }
}
