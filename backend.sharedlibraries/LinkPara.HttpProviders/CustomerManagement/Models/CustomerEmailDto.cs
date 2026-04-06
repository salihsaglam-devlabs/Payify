using LinkPara.HttpProviders.CustomerManagement.Models.Enums;


namespace LinkPara.HttpProviders.CustomerManagement.Models
{
    public class CustomerEmailDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public EmailType EmailType { get; set; }
        public bool Primary { get; set; }
    }
}
