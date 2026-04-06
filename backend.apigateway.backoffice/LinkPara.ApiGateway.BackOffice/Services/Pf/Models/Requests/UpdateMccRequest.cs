using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMccRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxIndividualInstallmentCount { get; set; }
    public int MaxCorporateInstallmentCount { get; set; }
    public string Description { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
