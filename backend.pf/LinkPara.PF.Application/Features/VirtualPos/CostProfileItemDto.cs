using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class CostProfileItemDto : IMapFrom<CostProfileItem>
{
    public Guid Id { get; set; }
    public CardTransactionType CardTransactionType { get; set; }
    public ProfileCardType ProfileCardType { get; set; }

    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }
    public bool? InstallmentSupport { get; set; }
    public Guid CostProfileId { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string CreatedBy { get; set; }
    public List<CostProfileInstallmentDto> CostProfileInstallments { get; set; } = new List<CostProfileInstallmentDto>();
}
