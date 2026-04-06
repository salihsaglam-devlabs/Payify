using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LinkPara.ContextProvider;

public class CurrentContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Context CurrentContext =>
        new()
        {
            UserId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserType = _httpContextAccessor?.HttpContext?.User?.FindFirst(q => q.Type == "UserType")?.Value,
            ClientIpAddress = _httpContextAccessor?.HttpContext?.Request?.Headers["ClientIpAddress"],
            Port = _httpContextAccessor?.HttpContext?.Request?.Headers["X-Forwarded-Port"],
            Gateway = _httpContextAccessor?.HttpContext?.Request?.Headers["Gateway"],
            Channel = _httpContextAccessor?.HttpContext?.Request?.Headers["Channel"],
            CanSeeSensitiveData = _httpContextAccessor?.HttpContext?.Request?.Headers["CanSeeSensitiveData"],
            CorrelationId = _httpContextAccessor?.HttpContext?.Request?.Headers["CorrelationId"],
            Language = _httpContextAccessor?.HttpContext?.Request?.Headers["Accept-Language"]
        };
}