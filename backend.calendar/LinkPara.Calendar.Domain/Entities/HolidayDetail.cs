using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Calendar.Domain.Entities;

public class HolidayDetail : AuditEntity, ITrackChange
{
    public int DurationInDays { get; set; }
    public DateTime DateOfHoliday { get; set; }
    public DateTime BeginningTime { get; set; }
    public DateTime EndingTime { get; set; }

    public Guid HolidayId { get; set; }
    public Holiday Holiday { get; set; }
}
