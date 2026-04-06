using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class OrderDto
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
    public EpinReconciliationStatus ReconciliationStatus { get; set; }
    public string Equivalent { get; set; }
    public string ErrorMessage { get; set; }
    public decimal UnitPrice { get; set; }
}
