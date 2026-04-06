using LinkPara.ApiGateway.CorporateWallet.Commons.Models.IdentityModels;

namespace LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

public interface IUserNameGenerator
{
    Task<string> GetUserNameAsync(string phoneCode, string phoneNumber);
    Task<string> GetUserNameAsync(string userName);
}

public class UserNameGenerator : IUserNameGenerator
{
    public async Task<string> GetUserNameAsync(string phoneCode, string phoneNumber)
    {
        return await Task.FromResult(
            string.Concat(UserTypePrefix.CorporateWallet, phoneCode, phoneNumber)
            .Replace("+", "").Replace(" ", "")
            );
    }

    public async Task<string> GetUserNameAsync(string userName)
    {
        return await Task.FromResult(
            string.Concat(UserTypePrefix.CorporateWallet, userName)
            .Replace("+", "").Replace(" ", "")
            );
    }
}
