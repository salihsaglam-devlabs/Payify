using LinkPara.Calendar.Application.Commons.Mappings;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.Calendar.Domain.Enums;

namespace LinkPara.Calendar.Application.Features.Holidays;

public class HolidayDto : IMapFrom<Holiday>
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; }
    public string Name { get; set; }
    public HolidayType HolidayType { get; set; }
}
