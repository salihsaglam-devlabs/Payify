namespace LinkPara.SharedModels.Banking.Enums;

public enum TransferType
{
    /// <summary>
    /// P2P
    /// </summary>
    Internal,

    /// <summary>
    /// Havale
    /// </summary>
    InsideBank,

    /// <summary>
    /// EFT
    /// </summary>
    Eft,

    /// <summary>
    /// FAST
    /// </summary>
    Fast,

    /// <summary>
    /// CREDIT CARD TOPUP
    /// </summary>
    CreditCardTopup
}
