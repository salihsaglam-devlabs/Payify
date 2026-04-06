namespace LinkPara.SharedModels.BusModels.Commands.BTrans;

public class PosInformationReport
{
    public string RecordType { get; set; }
    public string OperationType { get; set; }
    
    public string PosBankName { get; set; }
    public string PosBankCode { get; set; }
    public string PosMerchantId { get; set; }
    public string PosTerminalId { get; set; }
    
    public string SenderBankName { get; set; }
    public string SenderBankCode { get; set; }
    public string SenderIbanNumber { get; set; }
    public string SenderAccountNumber { get; set; }
    
    public bool IsReceiverCustomer { get; set; }
    public bool IsReceiverCorporate { get; set; }
    public string ReceiverTaxNumber { get; set; }
    public string ReceiverCommercialTitle { get; set; }
    public string ReceiverIdentityNumber { get; set; }
    public string ReceiverFirstName { get; set; }
    public string ReceiverLastName { get; set; }
    public string ReceiverNationCountryId { get; set; }
    public string ReceiverFullAddress { get; set; }
    public string ReceiverDistrict { get; set; }
    public string ReceiverPostalCode { get; set; }
    public string ReceiverCityId { get; set; }
    public string ReceiverCity { get; set; }
    public string ReceiverPhoneNumber { get; set; }
    public string ReceiverAccountNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string ReceiverBankName { get; set; }
    public string ReceiverBankCode { get; set; }
    public string ReceiverBranchName { get; set; }
    public string ReceiverIbanNumber { get; set; }
    
    public Guid RelatedTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal NetAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalPricingAmount { get; set; }
    public string CurrencyCode { get; set; }
    public string OrganizationDescription { get; set; }
    public string CustomerDescription { get; set; }
    
    public bool IsReceiverSubCompany { get; set; }
    public string HeadCompanyTaxNumber { get; set; }
    public string HeadCompanyCommercialTitle { get; set; }
    public decimal HeadCompanyPricingAmount { get; set; }
}