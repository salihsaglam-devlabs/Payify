
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class Publisher : AuditEntity
{
    public int ExternalId { get; set; }
    public string Name { get; set; }
}
