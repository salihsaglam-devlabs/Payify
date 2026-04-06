using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class RepresentativeTransactionDto
{
    public Guid Id { get; set; }
    public string SenderRepresentativeTitle { get; set; }
    public string SenderRepresentativeCode { get; set; }
    public string BeneficieryRepresentativeTitle { get; set; }
    public string BeneficieryRepresentativeCode { get; set; }
    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }
    public string SenderPhoneCode { get; set; }
    public string SenderPhoneNumber { get; set; }
    public string SenderIdentityNumber { get; set; }
    public string SenderEmail { get; set; }
    public string SenderAddress { get; set; }
    public string BeneficieryFirstName { get; set; }
    public string BeneficieryLastName { get; set; }
    public string BeneficieryPhoneCode { get; set; }
    public string BeneficieryPhoneNumber { get; set; }
    public string BeneficieryIdentityNumber { get; set; }
    public string BeneficieryEmail { get; set; }
    public string BeneficieryAddress { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime TransactionTime { get; set; }
    public RepresentativeTransactionStatus RepresentativeTransactionStatus { get; set; }
    public RepresentativeTransactionType RepresentativeTransactionType { get; set; }
    public string ReceiptNumber { get; set; }
    public string TotalAmountText { get; set; }
    public string SenderMersisNumber { get; set; }
    public string SenderTaxOffice { get; set; }
    public string SenderTaxNumber { get; set; }
}