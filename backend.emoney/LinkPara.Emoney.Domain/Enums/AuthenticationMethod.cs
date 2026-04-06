namespace LinkPara.Emoney.Domain.Enums;

public enum AuthenticationMethod
{
    NotDefined,
    None,
    Otp,
    _3D,
    Rta,
    Mpin,
    Mpin_And_Otp,
    NoAuth,
    DeviceFingerPrintOtp,
    CreditOtp,
    Cvv
}