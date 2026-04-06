using LinkPara.Calendar.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Calendar.Domain.Entities;

public class Holiday : AuditEntity, ITrackChange
{
    public string CountryCode { get; set; }
    public string Name { get; set; }
    public HolidayType HolidayType { get; set; }

    public List<HolidayDetail> HolidayDetails { get; set; } = new List<HolidayDetail>();
}
