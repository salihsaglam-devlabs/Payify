using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantBlockage : AuditEntity, ITrackChange
{
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public MerchantBlockageStatus MerchantBlockageStatus { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public List<MerchantBlockageDetail> MerchantBlockageDetails { get; set; }
}