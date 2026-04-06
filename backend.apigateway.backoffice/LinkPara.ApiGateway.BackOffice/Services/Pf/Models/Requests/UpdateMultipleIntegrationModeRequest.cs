using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMultipleIntegrationModeRequest
{
    public Guid MainSubMerchantId { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
}
