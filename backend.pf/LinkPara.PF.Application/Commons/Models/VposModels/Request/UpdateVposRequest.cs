using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence; 

namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class UpdateVposRequest : IMapFrom<Vpos>
{
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public SecurityType SecurityType { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public VposStatus VposStatus { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public List<SaveBankApiInfoDto> VposBankApiInfos { get; set; }
}
