using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Fraud.Domain.Entities;

public class TriggeredRule : AuditEntity
{
    public string RuleKey { get; set; }
    public int Score { get; set; }
    public Guid TransactionMonitoringId { get; set; }
    public TransactionMonitoring TransactionMonitoring { get; set; }
}
