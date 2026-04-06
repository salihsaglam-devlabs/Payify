using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantVpos : AuditEntity, ITrackChange
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public Guid VposId { get; set; }
    public Vpos Vpos { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string Password { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public int Priority { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public bool IsTerminalNotification { get; set; }
}
