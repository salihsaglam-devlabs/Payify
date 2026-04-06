namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

public enum LoginResult
{
    Succeeded,
    Failed,
    IsLockedOut,
    IsNotAllowed,
    RequiresTwoFactor
}