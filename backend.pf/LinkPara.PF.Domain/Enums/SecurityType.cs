namespace LinkPara.PF.Domain.Enums;

[Flags]
public enum SecurityType
{
    Unknown = 0,
    FullSecure = 1,
    HalfSecure = 2,
    NonSecure = 4
}
