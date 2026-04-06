using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;
public class UserDeviceInfo : AuditEntity
{
    public Guid UserId { get; set; }
    public bool IsMainDevice { get; set; }
    public Guid DeviceInfoId { get; set; }
    public DeviceInfo DeviceInfo { get; set; } 
}