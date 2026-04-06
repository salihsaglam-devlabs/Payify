using LinkPara.HttpProviders.CustomerManagement.Models.Enums;


namespace LinkPara.ApiGateway.Services.CustomerManagement.Models
{
    public class CustomerPhoneDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool Primary { get; set; }
        public PhoneType PhoneType { get; set; }
    }
}
