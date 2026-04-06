namespace LinkPara.SharedModels.Exceptions;

public class RecaptchaValidationException : CustomApiException
{
    public RecaptchaValidationException()
        : base(ErrorCode.InvalidRecaptcha, "Invalid Recaptcha Validation!") { }
}