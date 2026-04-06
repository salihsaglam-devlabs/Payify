namespace LinkPara.PF.Domain.Enums;
[Flags]
public enum PosProductType
{
    Unknown = 0,
    Pos = 1,
    Vpos = 2,
    WalletPos = 4
}