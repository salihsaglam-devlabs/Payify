using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.Address.Queries;
public class UserAddressDto : IMapFrom<UserAddress>
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