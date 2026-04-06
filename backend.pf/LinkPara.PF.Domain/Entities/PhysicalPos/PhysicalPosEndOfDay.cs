using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class PhysicalPosEndOfDay : AuditEntity
{
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public string BatchId { get; set; }
    public string PosMerchantId { get; set; }
    public string PosTerminalId { get; set; }
    public DateTime Date { get; set; }
    public int SaleCount { get; set; }
    public int VoidCount { get; set; }
    public int RefundCount { get; set; }
    public int InstallmentSaleCount { get; set; }
    public int FailedCount { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal VoidAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal InstallmentSaleAmount { get; set; }
    public string Currency { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string SerialNumber { get; set; }
    public EndOfDayStatus Status { get; set; }
}