using LinkPara.Fraud.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Fraud.Domain.Entities;

public class SearchLog : AuditEntity
{
    public string SearchName { get; set; }
    public string BirthYear { get; set; }
    public SearchType SearchType { get; set; }
    public MatchStatus MatchStatus { get; set; }
    public string ChannelType { get; set; }
    public int MatchRate { get; set; }
    public bool IsBlackList { get; set; }
    public string BlacklistName { get; set; }
    public string ClientIpAddress { get; set; }
    public string ReferenceNumber { get; set; }
    public DateTime? ExpireDate { get; set; }
}
