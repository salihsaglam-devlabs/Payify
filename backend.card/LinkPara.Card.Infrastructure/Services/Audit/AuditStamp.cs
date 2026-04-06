namespace LinkPara.Card.Infrastructure.Services.Audit;

public readonly record struct AuditStamp(string UserId, DateTime Timestamp);
