using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class SecurityQuestion : AuditEntity, ITrackChange    
{
    public string Question { get; set; }
    public string LanguageCode { get; set; }
}       