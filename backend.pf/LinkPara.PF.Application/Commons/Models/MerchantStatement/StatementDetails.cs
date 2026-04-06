namespace LinkPara.PF.Application.Commons.Models.MerchantStatement;

public class StatementDetails
{
    public StatementTenantDetails TenantDetails { get; set; }
    public StatementMerchantDetails MerchantDetails { get; set; }
    public StatementPeriod StatementPeriod { get; set; }
    public List<StatementTransaction> Transactions { get; set; }
    public StatementSummary StatementSummary { get; set; }
    public string ReceiptNumber { get; set; }
    public string Comments { get; set; }
    public string StatementDesign { get; set; }
}

public  class StatementTenantDetails
{
    public string CommercialTitle { get; set; }
    public string Address { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string Email { get; set; }
    public string Logo { get; set; }
    public string SignatureCircular { get; set; }
}

public class StatementMerchantDetails
{
    public string MerchantNumber { get; set; }
    public string MerchantAccountNumber { get; set; }
    public string MerchantWalletNumber { get; set; }
    public string MerchantName { get; set; }
    public string MerchantAddress { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
}

public class StatementPeriod
{
    public string Date { get; set; }
    public string Period { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}

public class StatementTransaction
{
    public string MerchantNumber { get; set; }
    public string AccountType { get; set; } = "Sanal Pos";
    public string TransactionDate { get; set; }
    public string ConversationId { get; set; }
    public string TransactionType { get; set; }
    public string CardNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal ChargebackAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public int InstallmentCount { get; set; }
    public decimal PointAmount { get; set; }
    public string PaymentDate { get; set; }
}

public class StatementSummary
{
    public decimal TotalAmount { get; set; }
    public decimal TotalCommissionAmount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public decimal TotalReceivedAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public decimal TotalReceivedNetAmount { get; set; }
}

