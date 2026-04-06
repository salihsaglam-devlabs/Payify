namespace LinkPara.HttpProviders.BTrans.Models.Enums;

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