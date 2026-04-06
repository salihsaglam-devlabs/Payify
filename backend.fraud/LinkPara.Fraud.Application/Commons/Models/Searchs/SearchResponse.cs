using LinkPara.Fraud.Domain.Enums;

namespace LinkPara.Fraud.Application.Commons.Models.Searchs;

public class SearchResponse
{
    public MatchStatus MatchStatus { get; set; }
    public int MatchRate { get; set; }
    public string ReferenceNumber { get; set; }
}
