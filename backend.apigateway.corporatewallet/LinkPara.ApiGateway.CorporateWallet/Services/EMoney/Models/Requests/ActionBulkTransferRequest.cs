namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class ActionBulkTransferRequest
{
    public Guid BulkTransferId { get; set; }
    public bool IsApprove { get; set; }
}
