using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class BankApiKeyDto
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string MappingName { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
