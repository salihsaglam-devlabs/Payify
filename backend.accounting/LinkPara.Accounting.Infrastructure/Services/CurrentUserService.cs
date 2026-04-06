using LinkPara.Accounting.Application.Commons.Interfaces;

namespace LinkPara.Accounting.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; }
}