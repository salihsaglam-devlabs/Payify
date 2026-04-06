using System.Text.Json;
using LinkPara.HttpProviders.Vault;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.Identity
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IVaultClient _vaultClient;
        private static readonly List<string> ChannelList = new List<string> { "backoffice", "mobile" };

        public RecaptchaService(IHttpClientFactory httpClientFactory,
            IVaultClient vaultClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _vaultClient = vaultClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> VerifyAsync(string recaptchaToken)
        {            
            var recaptchaSettings = new RecaptchaSettings();
            try
            {
                recaptchaSettings = await _vaultClient.GetSecretValueAsync<RecaptchaSettings>("SharedSecrets", "RecaptchaSettings");
            }
            catch
            {
                recaptchaSettings.RecaptchaControlEnabled = false;
            }            

            if (recaptchaSettings.RecaptchaControlEnabled)
            {
                var recaptchaResponse = new RecaptchaVerificationResponse();
                string channel = _httpContextAccessor?.HttpContext?.Request?.Headers["Channel"];
                if (!ChannelList.Contains(channel.ToLower()))
                {

                    var requestString = String.Concat(recaptchaSettings.ApiUrl, recaptchaSettings.SecretKey, "&response=", recaptchaToken);
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync(requestString, null);

                    var json = await response.Content.ReadAsStringAsync();
                    recaptchaResponse = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(json);

                    if (!recaptchaResponse.Success)
                    {
                        throw new RecaptchaValidationException();
                    }
                }
            }

            return true;
        }   
    }
}
       








