using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Screens
{
    public class ScreenDto : IMapFrom<Screen>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Module { get; set; }
        public string Icon { get; set; }
        public string ModuleIcon { get; set; }
        public string Link { get; set; }
        public string ModuleLink { get; set; }
        public int ModulePriority { get; set; }
        public int Priority { get; set; }
        public PermissionOperationType OperationType { get; set; }
    }
}
