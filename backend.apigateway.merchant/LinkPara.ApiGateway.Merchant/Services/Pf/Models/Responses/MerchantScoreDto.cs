namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantScoreDto
{
    public bool HasScoreCard { get; set; }
    public int? ScoreCardScore { get; set; }
    public bool HasFindeksRiskReport { get; set; }
    public int? FindeksScore { get; set; }
    public string AlexaRank { get; set; }
    public string GoogleRank { get; set; }
}
