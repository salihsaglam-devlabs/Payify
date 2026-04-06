

namespace LinkPara.IKS.Infrastructure.ExternalService.IKS.Models.Responses
{
    public class IKSTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int consented_on { get; set; }  
        public DateTime ExpireDate { get; set; }
    }
}
