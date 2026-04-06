using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantVposDto : IMapFrom<MerchantVpos>
{
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public VposModel Vpos { get; set; }
}

public class VposModel : IMapFrom<Vpos>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
}
