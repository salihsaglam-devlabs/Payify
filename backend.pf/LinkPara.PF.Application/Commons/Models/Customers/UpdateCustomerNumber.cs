namespace LinkPara.PF.Application.Commons.Models.Customers;

public class UpdateCustomerNumber
{
    public Guid CustomerId { get; set; }
    public Guid CustomerManagementId { get; set; }
    public int CustomerNumber { get; set; }
}