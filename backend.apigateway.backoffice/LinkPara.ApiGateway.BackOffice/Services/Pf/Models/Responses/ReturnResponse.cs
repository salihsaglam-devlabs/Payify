using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class ReturnResponse
    {
        public string ConversationId { get; set; }
        public string ReturnMessage { get; set; }
        public ReturnApprovalStatus ApprovalStatus { get; set; }
        public bool IsSucceed { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
