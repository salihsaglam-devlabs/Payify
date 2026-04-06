namespace LinkPara.ApiGateway.Services.Fraud.Models.Enums;

public enum MatchStatus
{
    Unknown,
    NoMatch,
    PotentialMatch,
    FalsePositive,
    TruePositive,
    TruePositiveApprove,
    TruePositiveReject
}
