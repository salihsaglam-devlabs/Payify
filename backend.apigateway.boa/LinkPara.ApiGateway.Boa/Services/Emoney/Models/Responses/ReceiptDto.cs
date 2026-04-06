using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class ReceiptDto
{
    public bool IsReady { get; set; }

    public string CustomerNumber { get; set; }
    public string ReceiptNumber { get; set; }

    public ReceiptParty Sender { get; set; }
    public ReceiptParty Receiver { get; set; }

    public ReceiptTransaction Transaction { get; set; }
    public ReceiptAmounts Amounts { get; set; }
    public ReceiptCompanyInfo CompanyInfo { get; set; }
}

public sealed class ReceiptAmounts
{
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Kmv { get; set; }
    public string TotalAmountText { get; set; }
}

public sealed class ReceiptCompanyInfo
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string MersisNumber { get; set; }
    public string TaxOffice { get; set; }
    public string TaxNumber { get; set; }
}

public sealed class ReceiptParty
{
    public string WalletNumber { get; set; }
    public string Iban { get; set; }
    public string FullName { get; set; }
    public string MaskedCardNumber { get; set; }
    public string BankName { get; set; }
}

public sealed class ReceiptTransaction
{
    public Guid TransactionId { get; set; }
    public string TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionDirection Direction { get; set; }

    public string CurrencyCode { get; set; }
    public string Display { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public string Tag { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
}