using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class BankTransactionDto
{
    public Guid Id { get; set; }
    public TimeoutTransactionType TransactionType { get; set; }
    public PfTransactionStatus TransactionStatus { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public int Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string CardNumber { get; set; }
    public bool IsReverse { get; set; }
    public DateTime ReverseDate { get; set; }
    public bool Is3ds { get; set; }
    public int IssuerBankCode { get; set; }
    public BankDto IssuerBank { get; set; }
    public int AcquireBankCode { get; set; }
    public BankDto AcquireBank { get; set; }
    public string MerchantCode { get; set; }
    public string SubMerchantCode { get; set; }
    public string BankOrderId { get; set; }
    public string RrnNumber { get; set; }
    public string ApprovalCode { get; set; }
    public string BankResponseCode { get; set; }
    public string BankResponseDescription { get; set; }
    public DateTime BankTransactionDate { get; set; }
    public DateTime TransactionStartDate { get; set; }
    public DateTime TransactionEndDate { get; set; }
    public Guid VposId { get; set; }
    public Guid MerchantTransactionId { get; set; }
}
