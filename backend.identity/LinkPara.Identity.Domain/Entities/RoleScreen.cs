using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Domain.Entities
{
    public class RoleScreen : AuditEntity
    {
        public Guid RoleId { get; set; }
        public Guid ScreenId { get; set; }
        public Role Role { get; set; }
        public Screen Screen { get; set; }
    }
}
