using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class UpdateCaseRequest
{
    public Guid Id { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
