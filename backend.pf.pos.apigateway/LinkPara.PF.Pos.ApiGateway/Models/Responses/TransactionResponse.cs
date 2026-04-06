namespace LinkPara.PF.Pos.ApiGateway.Models.Responses;

public class TransactionResponse : ResponseModel
{
    public string OrderId { get; set; }
    public string ProvisionNumber { get; set; }
    public string Description { get; set; }
}