namespace LinkPara.PF.Domain.Enums;

[Flags]
public enum SubQueryOptions
{
    None = 0,
    MerchantBankAccounts = 1,
    MerchantVposList = 2,
    MerchantIntegrator = 4,
    MerchantScores = 8,
    TechnicalContact = 16,
    MerchantDocuments = 32,
    MerchantUsers = 64,
    MerchantLimits = 128,
    MerchantApiKeyList = 256,
    MerchantWallets = 512
}