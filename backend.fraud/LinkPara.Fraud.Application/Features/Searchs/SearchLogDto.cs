using LinkPara.Fraud.Application.Commons.Mappings;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;

namespace LinkPara.Fraud.Application.Features.Searchs
{
    public class SearchLogDto : IMapFrom<SearchLog>
    {
        public string SearchName { get; set; }
        public string BirthYear { get; set; }
        public SearchType SearchType { get; set; }
        public MatchStatus MatchStatus { get; set; }
        public int MatchRate { get; set; }
        public bool IsBlackList { get; set; }
        public string BlacklistName { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string ClientIpAddress { get; set; }
    }
}
