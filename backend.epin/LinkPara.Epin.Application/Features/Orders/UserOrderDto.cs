using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Application.Features.Orders;

public class UserOrderDto : IMapFrom<Order>
{
    public Guid Id { get; set; }
    public string Equivalent { get; set; }
    public DateTime CreateDate { get; set; }
    public string Pin { get; set; }
    public decimal UnitPrice { get; set; }
    public string Image { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Order, UserOrderDto>()
            .ForMember(d => d.Image, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Image : string.Empty));
    }
}
