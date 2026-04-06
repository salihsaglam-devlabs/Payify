using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class Partner : AuditEntity
{
    public string Name { get; set; } 
    public string PartnerNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}
