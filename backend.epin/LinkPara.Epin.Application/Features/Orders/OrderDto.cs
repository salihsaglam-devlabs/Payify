using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Application.Features.Brands;
using LinkPara.Epin.Application.Features.Publishers;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;

namespace LinkPara.Epin.Application.Features.Orders;

public class OrderDto : IMapFrom<Order>
{
    public Guid Id { get; set; }
    public int ExternalProductId { get; set; }
    public string UserFullName { get; set; }
    public string Email { get; set; }
    public DateTime CreateDate { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal Total { get; set; }
    public Guid PublisherId { get; set; }
    public PublisherDto Publisher { get; set; }
    public Guid BrandId { get; set; }
    public BrandDto Brand { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public string Equivalent { get; set; }
    public string ErrorMessage { get; set; }
    public decimal UnitPrice { get; set; }
}
