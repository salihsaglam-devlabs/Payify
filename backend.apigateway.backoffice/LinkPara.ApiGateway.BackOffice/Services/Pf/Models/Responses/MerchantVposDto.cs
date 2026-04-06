using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantVposDto
{
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string ProviderKey { get; set; }
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public string BkmReferenceNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public VposModel Vpos { get; set; }
}

public class VposModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
}
