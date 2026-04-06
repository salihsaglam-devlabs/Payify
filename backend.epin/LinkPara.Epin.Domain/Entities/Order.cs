using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class Order : AuditEntity
{
    public int ExternalId { get; set; }
    public decimal Total { get; set; }
    public string Pin { get; set; }
    public int ExternalProductId { get; set; }
    public string ProvisionReferenceId { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string ReferenceId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; }
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public string Equivalent { get; set; }
    public string UserFullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime TransactionDate { get; set; }
}
