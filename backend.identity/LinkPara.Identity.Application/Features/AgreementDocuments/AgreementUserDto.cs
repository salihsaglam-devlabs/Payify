
using AutoMapper;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.AgreementDocuments
{
    public class AgreementUserDto : IMapFrom<User>
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
