using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
public class GetAllCustomerInformationsRequest : SearchQueryParams
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public KycSessionState? KycState { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
