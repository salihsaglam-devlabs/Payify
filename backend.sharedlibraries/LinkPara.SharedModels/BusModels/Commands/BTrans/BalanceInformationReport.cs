using LinkPara.SharedModels.BusModels.Commands.BTrans.Enums;

namespace LinkPara.SharedModels.BusModels.Commands.BTrans;

public class BalanceInformationReport
{
    public string RecordType { get; set; }
    public string OperationType { get; set; }
    public bool IsCorporate { get; set; }
    public string TaxNumber { get; set; }
    public string CommercialTitle { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? DocumentType { get; set; }
    public string IdentityNumber { get; set; }
    public string NationCountryId { get; set; }
    public string FullAddress { get; set; }
    public string District { get; set; }
    public string PostalCode { get; set; }
    public int? CityId { get; set; }
    public string City { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string WalletNumber { get; set; }
    public string CurrencyType { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public decimal AccountBalance { get; set; }
    public DateTime BalanceDate { get; set; }
}