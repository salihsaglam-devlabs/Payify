using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class ChargebackDto
{
    public Guid Id { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string WalletNumber { get; set; }
    public ChargebackStatus Status { get; set; }
    public string Description { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string MerchantId { get; set; }
    public string MerchantName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string OrderId { get; set; }
    public DateTime CreateDate { get; set; }
}
