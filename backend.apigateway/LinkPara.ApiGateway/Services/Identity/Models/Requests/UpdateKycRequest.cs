using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UpdateKycRequest
{
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string NationCountryId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string SerialNumber { get; set; }
    public string IdentityNumber { get; set; }
    public string Profession { get; set; }
}

public class UpdateKycServiceRequest : UpdateKycRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
