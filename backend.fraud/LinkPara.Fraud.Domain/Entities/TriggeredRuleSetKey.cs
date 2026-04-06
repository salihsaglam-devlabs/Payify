using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Fraud.Domain.Entities;

public class TriggeredRuleSetKey : AuditEntity
{
    public string Operation { get; set; }
    public string Level { get; set; }
    public string RuleSetKey { get; set; }
    public string ComplianceRuleSetKey { get; set; }
}
