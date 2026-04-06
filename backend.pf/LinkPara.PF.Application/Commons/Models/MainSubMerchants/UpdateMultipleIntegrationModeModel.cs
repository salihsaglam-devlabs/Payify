using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.MainSubMerchants;

public class UpdateMultipleIntegrationModeModel
{
    public Guid MainSubMerchantId { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
}
