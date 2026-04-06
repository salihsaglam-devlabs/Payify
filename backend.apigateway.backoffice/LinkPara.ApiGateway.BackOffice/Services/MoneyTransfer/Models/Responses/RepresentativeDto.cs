using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class RepresentativeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string TaxNumber { get; set; }
    public string AuthorizedUserFirstName { get; set; }
    public string AuthorizedUserLastName { get; set; }
    public string AuthorizedEmail { get; set; }
    public string AuthorizedPhoneCode { get; set; }
    public string AuthorizedPhoneNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public string MersisNumber { get; set; }
    public string TaxOffice{ get; set; }

}