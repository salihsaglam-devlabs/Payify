using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CostProfileItem : AuditEntity, ITrackChange
{
    public CardTransactionType CardTransactionType { get; set; }
    public ProfileCardType ProfileCardType { get; set; }
    
    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }   
    public bool? InstallmentSupport { get; set; }   
       
    public Guid CostProfileId { get; set; }
    public CostProfile CostProfile { get; set; }
    
    public virtual List<CostProfileInstallment> CostProfileInstallments { get; set; }
}