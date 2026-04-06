using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMerchantVposRequest
{
    public Guid Id { get; set; }
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public Guid MerchantId { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string CreatedBy { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
}
