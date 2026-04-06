using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Fraud.Domain.Entities;

public class IntegrationLog : AuditEntity
{
    public Guid TransactionMonitoringId { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public bool IsSuccess { get; set; }
    public TransactionMonitoring TransactionMonitoring { get; set; }
}
