using System.Threading;

namespace LinkPara.Card.Infrastructure.Services.Audit;

internal sealed class AuditUserContextAccessor : IAuditUserContextAccessor
{
    private static readonly AsyncLocal<string?> CurrentUser = new();

    public string? CurrentUserId
    {
        get => CurrentUser.Value;
        set => CurrentUser.Value = value;
    }
}
