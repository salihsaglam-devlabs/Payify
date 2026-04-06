using LinkPara.ApiGateway.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class DeleteUserInboxRequest
    {
        public List<Guid> InboxList { get; set; }
    }
}
