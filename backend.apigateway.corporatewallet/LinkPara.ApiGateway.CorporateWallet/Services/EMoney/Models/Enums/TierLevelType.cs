namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

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
    /// Custom Tier
    /// </summary>
    Custom
}