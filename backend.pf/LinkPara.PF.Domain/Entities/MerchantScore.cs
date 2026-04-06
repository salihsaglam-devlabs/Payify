using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantScore : AuditEntity
{
    public bool HasScoreCard { get; set; }
    public int? ScoreCardScore { get; set; }
    public bool HasFindeksRiskReport { get; set; }
    public int? FindeksScore { get; set; } 
    public string AlexaRank { get; set; }
    public string GoogleRank { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
}