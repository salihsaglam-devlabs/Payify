using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.DigitalKyc.Models.Requests;

public class KycUpdateRequest
{
    public DateTime DateOfBirth { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Profession { get; set; }
    public long IdentityNumber { get; set; }
}

public class KycUpdateServiceRequest : KycUpdateRequest, IHasUserId
{
    public Guid UserId { get; set; }
}