using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantDueDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid DueProfileId { get; set; }
    public DueProfileDto DueProfile { get; set; }
    public int TotalExecutionCount { get; set; }
    public DateTime LastExecutionDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
}