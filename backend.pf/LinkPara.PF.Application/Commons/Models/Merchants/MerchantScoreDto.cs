using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantScoreDto : IMapFrom<MerchantScore>
{
    public Guid Id { get; set; }
    public bool HasScoreCard { get; set; }
    public int? ScoreCardScore { get; set; }
    public bool HasFindeksRiskReport { get; set; }
    public int? FindeksScore { get; set; }
    public string AlexaRank { get; set; }
    public string GoogleRank { get; set; }
    public Guid MerchantId { get; set; }
    public string CreatedBy { get; set; }
}
