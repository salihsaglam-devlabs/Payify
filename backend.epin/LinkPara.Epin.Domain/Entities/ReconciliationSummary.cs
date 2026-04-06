using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class ReconciliationSummary : AuditEntity
{
    public DateTime ReconciliationDate { get; set; }
    public decimal ExternalTotal { get; set; }
    public decimal OrderTotal { get; set; }
    public int ExternalCount { get; set; }
    public int OrderCount { get; set; }
    public string Message { get; set; }
    public ReconciliationStatus ReconciliationStatus { get;set;}
    public Organization Organization { get; set; }
    public List<ReconciliationDetail> ReconciliationDetails { get; set; }
}
