using LinkPara.PF.Domain.Entities.PhysicalPos;

namespace LinkPara.PF.Application.Commons.Models.PhysicalPos;

public class CreateOrUpdateEndOfDayResponse
{
    public bool IsSucceeded { get; set; }
    public string ErrorCode {get; set;}
    public bool IsCreated { get; set; }
    public PhysicalPosEndOfDay PhysicalPosEndOfDay { get; set; }
}