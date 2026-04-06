using LinkPara.Identity.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Screens.Queries
{
    public class RoleScreenDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public RoleScope RoleScope { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public List<ScreenDto> Screens { get; set; }
    }
}
