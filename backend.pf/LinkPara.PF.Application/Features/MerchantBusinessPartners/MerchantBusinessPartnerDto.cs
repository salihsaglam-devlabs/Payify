using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners
{
    public class MerchantBusinessPartnerDto : IMapFrom<MerchantBusinessPartner>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid MerchantId { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
