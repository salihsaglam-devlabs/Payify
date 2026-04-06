using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class Field : AuditEntity
{
    public string Label { get; set; }
    public string Mask { get; set; }
    public string Pattern { get; set; }
    public string Placeholder { get; set; }
    public int Length { get; set; }
    public int Order { get; set; }
    public string Prefix { get; set; }
    public string Suffix { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
}