using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.RepresentativeTransactions;

public class SaveRepresentativeTransactionRequest
{
    public Guid SenderRepresentativeId { get; set; }
    public Guid BeneficieryRepresentativeId { get; set; }
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
    public RepresentativeTransactionType RepresentativeTransactionType { get; set; }
}