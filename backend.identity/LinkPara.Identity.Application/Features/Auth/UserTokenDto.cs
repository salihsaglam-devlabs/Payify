using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.OAuth
{
    public class UserTokenDto
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public bool UserSecurityPictureEnabled { get; set; }
        public byte[] UserSecurityPicture { get; set; }
    }
}
