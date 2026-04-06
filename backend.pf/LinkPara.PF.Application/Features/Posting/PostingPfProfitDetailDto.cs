using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.Posting;

public class PostingPfProfitDetailDto : IMapFrom<PostingPfProfitDetail>
{
    public Guid Id { get; set; }
    public int AcquireBankCode { get; set; }
    public string BankName { get; set; }
    public decimal BankPayingAmount { get; set; }
    public int Currency { get; set; }
    public Guid PostingPfProfitId { get; set; }
}