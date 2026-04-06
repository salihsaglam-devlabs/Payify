using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments;

public class RouteResponse : ResponseBase
{
    public CardTransactionType CardTransactionType { get; set; }
    public Vpos Vpos { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsOptionalPos { get; set; }
    public bool IsHealthCheckOptionalPos { get; set; }
    public Guid CostProfileItemId { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public List<InstallmentItem> Installments { get; set; }
}

public class InstallmentItem
{
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
}