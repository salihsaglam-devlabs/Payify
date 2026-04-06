using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Infrastructure.Services
{
    public class ScreenService : IScreenService
    {
        private readonly ApplicationDbContext _context;

        public ScreenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteRoleScreenAsync(RoleScreen roleScreen)
        {
            _context.RoleScreen.Remove(roleScreen);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteRoleScreensByRoleIdAsync(Guid roleId)
        {
            var roleScreensToDelete = _context.RoleScreen.Where(rs => rs.RoleId == roleId);
            _context.RoleScreen.RemoveRange(roleScreensToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
