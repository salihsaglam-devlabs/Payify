using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class ActivationRequest
{
    public Guid Id { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
