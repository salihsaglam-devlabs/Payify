using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.Posting;

public class PublishPostingItem
{
    public PostingItem PostingItem { get; set; }
    public List<Guid> MerchantTransactionIds { get; set; }
}