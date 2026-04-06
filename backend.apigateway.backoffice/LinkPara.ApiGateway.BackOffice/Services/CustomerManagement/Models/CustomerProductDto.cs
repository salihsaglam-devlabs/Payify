using LinkPara.HttpProviders.CustomerManagement.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models
{
    public class CustomerProductDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime ReopeningDate { get; set; }
        public Guid AccountId { get; set; }
        public Guid MerchantId { get; set; }
        public CustomerProductStatus CustomerProductStatus { get; set; }
        public DateTime SuspendedDate { get; set; }
    }
}
