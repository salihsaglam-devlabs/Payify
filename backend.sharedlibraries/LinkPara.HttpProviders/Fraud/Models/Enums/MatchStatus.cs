namespace LinkPara.HttpProviders.Fraud.Models.Enums;

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
