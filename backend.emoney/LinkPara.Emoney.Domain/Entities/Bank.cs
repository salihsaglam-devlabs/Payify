using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class Bank : AuditEntity
{ 
    public int Code { get; set; }
    public string Name { get; set; }
    
    [NotMapped]
    public bool HasLogo { get; set; }
}