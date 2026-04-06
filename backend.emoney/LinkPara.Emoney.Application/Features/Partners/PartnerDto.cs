using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Partners
{
    public class PartnerDto : IMapFrom<Partner>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PartnerNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
