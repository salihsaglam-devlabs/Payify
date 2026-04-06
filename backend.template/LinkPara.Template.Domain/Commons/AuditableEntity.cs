using System.ComponentModel.DataAnnotations;
using LinkPara.Template.Domain.Enums;

namespace LinkPara.Template.Domain.Commons;

public class AuditableEntity
{
    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset? UpdateDate { get; set; }
    
    [MaxLength(50)]
    public string CreatedBy { get; set; }

    [MaxLength(50)]
    public string LastModifiedBy { get; set; }
    
    public RecordStatus RecordStatus { get; set; }
}