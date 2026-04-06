using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class VposDto : IMapFrom<Vpos>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public SecurityType SecurityType { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public virtual List<VposBankApiInfoDto> VposBankApiInfos { get; set; }
    public bool? HasActiveCostProfile { get; set; }
}
