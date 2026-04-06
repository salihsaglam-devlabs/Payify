namespace LinkPara.HttpProviders.Identity;

public interface IRecaptchaService
{
    Task<bool> VerifyAsync(string recaptchaToken);
}