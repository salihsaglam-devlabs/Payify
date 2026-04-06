using LinkPara.Audit.Models;

namespace LinkPara.Audit;
public interface IAuditLogService
{
    public Task AuditLogAsync(AuditLog auditLog);
}
