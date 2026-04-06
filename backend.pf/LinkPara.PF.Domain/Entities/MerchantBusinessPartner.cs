using LinkPara.SharedModels.Persistence;


namespace LinkPara.PF.Domain.Entities
{
    public class MerchantBusinessPartner : AuditEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid MerchantId { get; set; }
        public string AmlReferenceNumber { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}
