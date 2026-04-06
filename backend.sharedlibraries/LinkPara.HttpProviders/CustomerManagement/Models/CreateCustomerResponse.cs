namespace LinkPara.HttpProviders.CustomerManagement.Models
{
    public class CreateCustomerResponse
    {
        public Guid CustomerId { get; set; }
        public int CustomerNumber { get; set; }
        public bool IsChanged { get; set; }
        public CustomerDto Customer { get; set; }
    }
}
