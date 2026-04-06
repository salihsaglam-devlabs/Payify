using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.PhysicalPoses;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices;

public class MerchantPhysicalPosDto : IMapFrom<MerchantPhysicalPos>
{
    public Guid Id { get; set; }
    public Guid MerchantPhysicalDeviceId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public string PosMerchantId { get; set; } 
    public string PosTerminalId { get; set; } 
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public PhysicalPosDto PhysicalPos { get; set; }
}
