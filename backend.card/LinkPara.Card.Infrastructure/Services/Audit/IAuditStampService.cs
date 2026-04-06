using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Services.Audit;

public interface IAuditStampService
{
    AuditStamp CreateStamp();
    void StampForCreate(AuditEntity entity);
    void StampForCreate(IEnumerable<AuditEntity> entities);
    void StampForUpdate(AuditEntity entity);
    void StampForUpdate(IEnumerable<AuditEntity> entities);
}
