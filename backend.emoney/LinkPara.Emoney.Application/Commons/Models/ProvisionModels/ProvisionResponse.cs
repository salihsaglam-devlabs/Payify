namespace LinkPara.Emoney.Application.Commons.Models.ProvisionModels;

public class ProvisionResponse : ResponseBase
{
    public string ConversationId { get; set; }
    public string ReferenceNumber { get; set; }
    public string ProvisionReference { get; set; }
    public Guid TransactionId { get; set; }
}