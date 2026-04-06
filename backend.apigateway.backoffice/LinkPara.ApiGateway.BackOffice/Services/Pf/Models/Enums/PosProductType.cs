namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

[Flags]
public enum PosProductType
{
    Undefined = 0,
    Pos = 1,
    Vpos = 2,
    WalletPos = 4
}