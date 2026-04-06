using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Identity.Models.Enums;


namespace LinkPara.HttpProviders.CustomerManagement.Models
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public int CustomerNumber { get; set; }
        public CustomerType CustomerType { get; set; }
        public string CommercialTitle { get; set; }
        public string TradeRegistrationNumber { get; set; }
        public string TaxAdministration { get; set; }
        public string TaxNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DocumentType DocumentType { get; set; }
        public string SerialNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string Profession { get; set; }
        public string NationCountryId { get; set; }
        public string NationCountry { get; set; }
        public string NewCustomerId { get; set; }
        public List<CustomerProductDto> CustomerProducts { get; set; }
        public List<CustomerAddressDto> CustomerAddresses { get; set; }
        public List<CustomerPhoneDto> CustomerPhones { get; set; }
        public List<CustomerEmailDto> CustomerEmails { get; set; }
    }
}
