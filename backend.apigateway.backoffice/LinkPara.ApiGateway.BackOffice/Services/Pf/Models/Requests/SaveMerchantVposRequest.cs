using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveMerchantVposRequest
{
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
}
