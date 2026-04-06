using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.Identity.Models;

public class GetAppUsersRequest
{
    public string Email { get; set; }
    public string UserName { get; set; }
}