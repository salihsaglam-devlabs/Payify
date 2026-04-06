using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserPicture : AuditEntity
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}