using LinkPara.Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public  interface IAuthService
    {
        Task DeleteRefreshToken(UserSession session);
        Task UpdateRefreshTokenExpirationTime(UserSession session, TimeSpan jwtExpiration);
    }
}
