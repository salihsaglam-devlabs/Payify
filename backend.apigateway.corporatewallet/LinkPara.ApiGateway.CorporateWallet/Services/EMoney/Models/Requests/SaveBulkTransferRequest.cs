using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class SaveBulkTransferRequest
{
    public string FileName { get; set; }
    public string SenderWalletNumber { get; set; }
    public BulkTransferType BulkTransferType { get; set; }
    public List<BulkTransferDetailRequest> BulkTransferDetails { get; set; }
}

