using LinkPara.Emoney.Application.Commons.Interfaces;

namespace LinkPara.Emoney.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; }
}