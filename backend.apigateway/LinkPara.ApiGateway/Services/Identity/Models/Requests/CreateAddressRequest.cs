using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class CreateAddressRequest
{
    public int CountryId { get; set; }
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Neighbourhood { get; set; }
    public string Street { get; set; }
    public string FullAddress { get; set; }
}
public class CreateAddressServiceRequest : CreateAddressRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
