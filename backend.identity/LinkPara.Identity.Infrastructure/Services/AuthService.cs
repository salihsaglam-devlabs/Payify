using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Infrastructure.Persistence;

namespace LinkPara.Identity.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteRefreshToken(UserSession session)
        {
            _context.UserSession.Remove(session);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRefreshTokenExpirationTime(UserSession session, TimeSpan jwtExpiration)
        {
            session.RefreshTokenExpiration = session.RefreshTokenExpiration.Add(jwtExpiration);
            _context.UserSession.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}
