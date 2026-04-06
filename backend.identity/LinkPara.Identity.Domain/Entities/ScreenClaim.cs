using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Domain.Entities
{
    public class ScreenClaim : AuditEntity
    {
        public Guid ScreenId { get; set; }
        public string ClaimValue { get; set; }
        public Screen Screen { get; set; }
    }
}
