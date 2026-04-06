using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class EpinReconciliationDetailDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; }
    public string Email { get; set; }
    public string TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string PublisherName { get; set; }
    public string BrandName { get; set; }
    public string ProductName { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public EpinReconciliationDetailStatus ReconciliationDetailStatus { get; set; }
    public int ExternalOrderId { get; set; }
    public bool HasInternalOrders { get; set; }
    public bool HasExternalOrders { get; set; }
    public Guid? OrderId { get; set; }
}
