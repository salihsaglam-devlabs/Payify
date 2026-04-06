using LinkPara.SharedModels.Persistence;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Domain.Entities;
public class Vpos : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public VposStatus VposStatus { get;set;}
    public Guid AcquireBankId { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public SecurityType SecurityType { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public List<VposBankApiInfo> VposBankApiInfos { get; set; }
    public List<CostProfile> CostProfiles { get; set; }
    public List<VposCurrency> Currencies { get; set; }
    public List<MerchantVpos> MerchantVposList { get; set; }
}