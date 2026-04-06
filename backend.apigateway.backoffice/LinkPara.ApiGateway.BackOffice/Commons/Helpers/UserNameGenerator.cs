using LinkPara.ApiGateway.BackOffice.Commons.Models.IdentityModels;

namespace LinkPara.ApiGateway.Commons.Helpers;

public interface IUserNameGenerator
{
    Task<string> GetUserName(string userName, string channel);
    Task<string> GetUserName(string prefix, string phoneCode, string phoneNumber);
}

public class UserNameGenerator : IUserNameGenerator
{
    private const string MerchantPortal = "MerchantPortal";

    public async Task<string> GetUserName(string userName, string channel)
    {
        if (channel is null)
        {
            return await Task.FromResult(string.Concat(UserTypePrefix.Internal, userName.Replace("+", "")));
        }
        else
        {
            var username = channel == MerchantPortal
                ? string.Concat(UserTypePrefix.Corporate, userName)
                : string.Concat(UserTypePrefix.Internal, userName);

            return await Task.FromResult(username.Replace("+", ""));
        }
    }

    public async Task<string> GetUserName(string prefix, string phoneCode, string phoneNumber)
    {
        return await Task.FromResult(string.Concat(prefix, phoneCode, phoneNumber).Replace("+", ""));
    }
}
