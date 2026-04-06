using LinkPara.PF.Application.Commons.Interfaces;

namespace LinkPara.PF.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; }
}