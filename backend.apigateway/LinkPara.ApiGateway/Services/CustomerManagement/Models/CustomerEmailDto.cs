using LinkPara.HttpProviders.CustomerManagement.Models.Enums;


namespace LinkPara.ApiGateway.Services.CustomerManagement.Models
{
    public class CustomerEmailDto
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public EmailType EmailType { get; set; }
        public bool Primary { get; set; }
    }
}
