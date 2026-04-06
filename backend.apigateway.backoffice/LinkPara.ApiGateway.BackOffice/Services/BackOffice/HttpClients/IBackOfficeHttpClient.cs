using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.BackOffice.HttpClients;

public interface IBackOfficeHttpClient
{
    Task<string> RecallApprovedRequestAsync(RequestDto request);
}
