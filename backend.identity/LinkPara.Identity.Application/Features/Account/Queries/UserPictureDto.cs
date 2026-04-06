using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.Account.Queries
{
    public class UserPictureDto: IMapFrom<UserPicture>
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public Guid UserId { get; set; }
    }
}
