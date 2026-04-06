namespace LinkPara.ApiGateway.Services.Identity.Models.Responses;

public class UserAddressDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public int CountryId { get; set; }
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Neighbourhood { get; set; }
    public string Street { get; set; }
    public string FullAddress { get; set; }
}
