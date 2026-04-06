using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class WithdrawRequestAdminDto
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public WithdrawStatus WithdrawStatus { get; set; }
    public TransferType TransferType { get; set; }
    public string WalletNumber { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsReceiverIbanOwned { get; set; }
    public string ReceiverIbanNumber { get; set; }
    public string ReceiverName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string BankReferenceNumber { get; set; }
    public string TransactionId { get; set; }
    public string QueryNumber { get; set; }
    public string TransactionResponse { get; set; }
    public Guid AccountId { get; set; }
}