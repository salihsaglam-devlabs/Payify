namespace LinkPara.HttpProviders.Identity.Models;

public class RecaptchaSettings
{
    public string ApiUrl { get; set; }
    public string SecretKey { get; set; }
    public bool RecaptchaControlEnabled { get; set; }
}