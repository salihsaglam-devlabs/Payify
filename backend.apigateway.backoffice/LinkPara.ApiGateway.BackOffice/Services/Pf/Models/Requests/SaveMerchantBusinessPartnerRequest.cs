namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class SaveMerchantBusinessPartnerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid MerchantId { get; set; }
    }
}
