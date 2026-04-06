namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums;

public enum TierLevelType
{
    /// <summary>
    /// Non-KYC
    /// </summary>
    Tier0,

    /// <summary>
    ///  Pre-KYC 
    /// </summary>
    Tier1,

    /// <summary>
    /// KYC
    /// </summary>
    Tier2,
        
    /// <summary>
    /// Premium
    /// </summary>
    Tier3,
        
    /// <summary>
    /// PremiumPlus
    /// </summary>
    Tier4,

    /// <summary>
    /// ChildKyc
    /// </summary>
    Tier5,
    /// <summary>
    /// Custom Tier
    /// </summary>
    Custom,
    /// <summary>
    /// Corporate Tier
    /// </summary>
    Corporate
}