using LinkPara.Calendar.Application.Commons.Mappings;
using LinkPara.Calendar.Application.Features.Holidays;
using LinkPara.Calendar.Domain.Entities;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Queries;

public class HolidayDetailDto : IMapFrom<HolidayDetail>
{
    public Guid Id { get; set; }
    public int DurationInDays { get; set; }
    public DateTime DateOfHoliday { get; set; }
    public DateTime BeginningTime { get; set; }
    public DateTime EndingTime { get; set; }
    public Guid HolidayId { get; set; }
    public HolidayDto Holiday { get; set; }
}
