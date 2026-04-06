using LinkPara.Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public  interface IScreenService
    {
        Task DeleteRoleScreenAsync(RoleScreen roleScreen);
        Task DeleteRoleScreensByRoleIdAsync(Guid roleId);
    }
}
