namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Enum;

public enum DocumentStatus
{
    Processing,
    Created,
    Sent,
    PartiallyCompleted,
    Completed,
    FailedOnCreate,
    FailedOnUpload,
    Reverted,
    RevertFailed
}