using Microsoft.AspNetCore.Identity;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Identity.Domain.Entities;

public class RoleClaim : IdentityRoleClaim<Guid>, ITrackChange
{
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string LastModifiedBy { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}