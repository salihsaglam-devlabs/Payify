using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.BulkOperations.Merchants;

public class ExcelMerchantModel
{
    public int Index { get; set; }
    public string MerchantName { get; set; }
    public string WebSiteUrl { get; set; }
    public string MccCode { get; set; }
    public string PhoneCode { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public ExcelMerchantCustomerModel Customer { get; set; }
    public ExcelMerchantContactPersonModel TechnicalContact { get; set; }
    public ExcelMerchantAdminUserModel AdminUser { get; set; }
}

public class ExcelMerchantCustomerModel
{
    public CompanyType CompanyType { get; set; }
    public string CommercialTitle { get; set; }
    public string TradeRegistrationNumber { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public int Country { get; set; }
    public int City { get; set; }
    public int District { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public ExcelMerchantContactPersonModel AuthorizedPerson { get; set; }
}

public class ExcelMerchantContactPersonModel
{
    public string IdentityNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }
    public string CompanyPhoneNumber { get; set; }
    public string MobilePhoneNumber { get; set; }
}

public class ExcelMerchantAdminUserModel
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public DateTime BirthDate { get; set; }
}