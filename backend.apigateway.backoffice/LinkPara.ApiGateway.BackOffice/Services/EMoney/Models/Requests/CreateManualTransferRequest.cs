using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class CreateManualTransferRequest
{
    public string CustomerWalletNumber { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }

    public Guid TransferRequestFile { get; set; }
    public Guid? TransferApprovalFile { get; set; }
    public string Description { get; set; }
}