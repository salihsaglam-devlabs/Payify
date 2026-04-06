namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class ProvisionResponse : ResponseBase
{
    public Guid TransactionId { get; set; }
    public string OrderId { get; set; }
    public string ProvisionNumber { get; set; }
    public string Description { get; set; }
}
