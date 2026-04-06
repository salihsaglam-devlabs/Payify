using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantBlockageDetail : AuditEntity
{   
    public DateTime PostingDate { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public Guid MerchantBlockageId { get; set; }
    public MerchantBlockage MerchantBlockage { get; set; }
}