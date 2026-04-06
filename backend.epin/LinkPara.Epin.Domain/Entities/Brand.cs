using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Epin.Domain.Entities;

public class Brand : AuditEntity
{
    public int ExternalId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Image { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; }
}
