using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantContentVersion : AuditEntity, ITrackChange
{
    public Guid MerchantContentId { get; set; }
    public MerchantContent MerchantContent { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string LanguageCode { get; set; }
}