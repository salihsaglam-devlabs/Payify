using LinkPara.ApiGateway.Boa.Commons.IdentityModels;

namespace LinkPara.ApiGateway.Boa.Commons.Helpers;

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
            string.Concat(UserTypePrefix.Individual, phoneCode, phoneNumber)
            .Replace("+", "")
            );
    }

    public async Task<string> GetUserNameAsync(string userName)
    {
        return await Task.FromResult(
            string.Concat(UserTypePrefix.Individual, userName)
            .Replace("+", "")
            );
    }
}
