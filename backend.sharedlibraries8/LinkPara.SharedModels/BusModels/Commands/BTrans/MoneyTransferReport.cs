using LinkPara.SharedModels.BusModels.Commands.BTrans.Enums;

namespace LinkPara.SharedModels.BusModels.Commands.BTrans;

public class MoneyTransferReport
{
    public string RecordType { get; set; }
    public OperationType OperationType { get; set; }
    public TransferType TransferType { get; set; }
    
    public bool IsSenderCustomer { get; set; }
    public bool IsSenderCorporate { get; set; }
    public string SenderTaxNumber { get; set; }
    public string SenderCommercialTitle { get; set; }
    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }
    public int? SenderDocumentType { get; set; }
    public string SenderIdentityNumber { get; set; }
    public string SenderNationCountryId { get; set; }
    public string SenderFullAddress { get; set; }
    public string SenderDistrict { get; set; }
    public string SenderPostalCode { get; set; }
    public int SenderCityId { get; set; }
    public string SenderCity { get; set; }
    public string SenderPhoneNumber { get; set; }
    public string SenderEmail { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderBankName { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderBranchName { get; set; }
    public string SenderIbanNumber { get; set; }
    public string SenderCreditCard { get; set; }
    public string SenderDebitCard { get; set; }

    public bool IsReceiverCustomer { get; set; }
    public bool IsReceiverCorporate { get; set; }
    public string ReceiverTaxNumber { get; set; }
    public string ReceiverCommercialTitle { get; set; }
    public string ReceiverFirstName { get; set; }
    public string ReceiverLastName { get; set; }
    public int? ReceiverDocumentType { get; set; }
    public string ReceiverIdentityNumber { get; set; }
    public string ReceiverNationCountryId { get; set; }
    public string ReceiverFullAddress { get; set; }
    public string ReceiverDistrict { get; set; }
    public string ReceiverPostalCode { get; set; }
    public int ReceiverCityId { get; set; }
    public string ReceiverCity { get; set; }
    public string ReceiverPhoneNumber { get; set; }
    public string ReceiverEmail { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string ReceiverBankName { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverBranchName { get; set; }
    public string ReceiverIbanNumber { get; set; }
    
    public Guid RelatedTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string IpAddress { get; set; }
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string CurrencyCode { get; set; }
    public decimal TotalPricingAmount { get; set; }
    public TransferReason TransferReason { get; set; }
    public string CustomerDescription { get; set; }
}