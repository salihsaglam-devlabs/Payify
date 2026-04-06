using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserPasswordHistory : AuditEntity    
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string PasswordHash { get; set; }
}   