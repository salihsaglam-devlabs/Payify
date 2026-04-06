using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;

public class AccountingCustomerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ResultMessage { get; set; }
    public DateTime CreateDate { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
}
