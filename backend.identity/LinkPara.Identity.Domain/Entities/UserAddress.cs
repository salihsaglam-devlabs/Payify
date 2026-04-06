using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserAddress : AuditEntity, ITrackChange   
{
    public User User { get; set; }
    public Guid UserId { get; set; }
    public int CountryId { get; set; }
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Neighbourhood { get; set; }
    public string Street { get; set; }
    public string FullAddress { get; set; }
}
