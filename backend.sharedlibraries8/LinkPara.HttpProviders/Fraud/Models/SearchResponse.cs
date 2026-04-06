using LinkPara.HttpProviders.Fraud.Models.Enums;

namespace LinkPara.HttpProviders.Fraud.Models;

public class SearchResponse
{
    public MatchStatus MatchStatus { get; set; }
    public int MatchRate { get; set; }
    public string ReferenceNumber { get; set; }
}
