using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class SaveMerchantVposRequest : IMapFrom<MerchantVpos>
{
    public Guid Id { get; set; }
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public Guid MerchantId { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string CreatedBy { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public bool IsTerminalNotification { get; set; }
}

