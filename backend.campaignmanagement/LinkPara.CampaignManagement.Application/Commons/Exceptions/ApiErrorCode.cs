using LinkPara.SharedModels.Exceptions;

namespace LinkPara.CampaignManagement.Application.Commons.Exceptions;

/// <summary>
///  CampaignManagement Api Error Codes
/// </summary>
public class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.CampaignManagement;

    public const string ProvisionPreviewError = _prefix + "001";
    public const string ProvisionError = _prefix + "002";
}
