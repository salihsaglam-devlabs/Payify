
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Approval.Application.Commons.Exceptions;

/// <summary>
///  Approval Api Error Codes
/// </summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Approval;

    public const string MakerAndCheckerAreSame = _prefix + "001";
    public const string NotRelevantApprover = _prefix + "002";
    public const string InvalidStatus = _prefix + "003";
    public const string SameRecord = _prefix + "004";
}