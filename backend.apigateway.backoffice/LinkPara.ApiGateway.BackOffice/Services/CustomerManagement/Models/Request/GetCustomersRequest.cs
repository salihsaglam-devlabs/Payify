using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Request
{
    public class GetCustomersRequest : SearchQueryParams
    {
        public int? CustomerNumber { get; set; }
        public string FullName { get; set; }
        public string NationCountryId { get; set; }
        public string IdentityNumber { get; set; }
        public string CommercialTitle { get; set; }
        public string TradeRegistrationNumber { get; set; }
        public string TaxAdministration { get; set; }
        public string TaxNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MersisNumber { get; set; }
        public ProductType?[] ProductType { get; set; }
        public CustomerType? CustomerType { get; set; }
        public DocumentType? DocumentType { get; set; }
    }

}
