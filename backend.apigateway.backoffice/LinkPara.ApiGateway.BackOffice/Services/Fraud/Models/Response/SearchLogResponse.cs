using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Response
{
    public class SearchLogResponse
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