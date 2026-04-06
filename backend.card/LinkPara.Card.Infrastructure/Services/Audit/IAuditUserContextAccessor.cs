namespace LinkPara.Card.Infrastructure.Services.Audit;

public interface IAuditUserContextAccessor
{
    string? CurrentUserId { get; set; }
}
