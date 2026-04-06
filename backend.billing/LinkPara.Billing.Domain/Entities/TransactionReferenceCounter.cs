using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Domain.Entities;

public class TransactionReferenceCounter : AuditEntity
{
    /// <summary>
    /// ReferenceId int Value that should be sent to bank per request 
    /// </summary>
    public int TransactionReferenceInt { get; set; }

    /// <summary>
    /// ReferenceId guid Value that should be sent to bank per request
    /// </summary>
    public Guid TransactionReferenceGuid { get; set; } = Guid.NewGuid();
}
