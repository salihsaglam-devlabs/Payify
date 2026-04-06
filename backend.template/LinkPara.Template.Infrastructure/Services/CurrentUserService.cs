using LinkPara.Template.Application.Commons.Interfaces;

namespace LinkPara.Template.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; }
}