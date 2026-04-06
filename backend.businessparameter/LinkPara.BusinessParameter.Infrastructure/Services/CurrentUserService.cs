using LinkPara.BusinessParameter.Application.Commons.Interfaces;

namespace LinkPara.BusinessParameter.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; }
}