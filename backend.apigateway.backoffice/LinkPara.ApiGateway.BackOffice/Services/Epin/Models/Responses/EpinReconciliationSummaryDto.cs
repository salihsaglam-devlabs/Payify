using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class EpinReconciliationSummaryDto
{
    public Guid Id { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public decimal ExternalTotal { get; set; }
    public decimal OrderTotal { get; set; }
    public int ExternalCount { get; set; }
    public int OrderCount { get; set; }
    public string Message { get; set; }
    public EpinReconciliationStatus ReconciliationStatus { get; set; }
    public Organization Organization { get; set; }
    public List<EpinReconciliationDetailDto> UnreconciledOrders { get; set; }
}
